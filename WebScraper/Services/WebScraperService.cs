using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using System.Net;
using WebScraper.Models;
using WebScraper.Services.Interfaces;
using MongoDB.Bson;

namespace WebScraper.Services;

class WebScraperService : IWebScraperService
{
    private readonly IScrapConfigurationProvider _scrapConfiguration;
    private readonly IProductRepository _productRepository;
    private readonly HttpClient _http = new HttpClient();

    public WebScraperService(
        IScrapConfigurationProvider scrapConfiguration,
        IProductRepository productRepository)
    {
        _scrapConfiguration = scrapConfiguration;
        _productRepository = productRepository;
    }

    public async Task Scrap(string url, ScrapConfiguration scrapConfiguration = null)
    {
        var config = scrapConfiguration ?? _scrapConfiguration.GetDefaultConfiguration(url);

        // Taking all category links
        var start = Load(url);
        var categoryLinks = start.DocumentNode
            .QuerySelectorAll(config.Category.CategorySelector)
            .Select(a => a.GetAttributeValue("href", null))
            .Where(h => !string.IsNullOrWhiteSpace(h))
            .Distinct()
            .ToList();

        var items = new List<Product>();

        foreach (var categoryUrl in categoryLinks)
        {
            var currentPageUrl = categoryUrl;

            while (true)
            {
                var doc = Load(currentPageUrl);

                var products = doc.DocumentNode.QuerySelectorAll(config.Product.ProductContainerSelector);
                foreach (var product in products)
                {
                    var title = product.QuerySelector(config.Product.ProductTitleSelector)?.InnerText?.Trim() ?? "";
                    var price = product.QuerySelector(config.Product.ProductPriceSelector)?.InnerText?.Trim() ?? "";

                    var sku = config.ScrapProductPage
                        ? GetSkuFromProduct(product.QuerySelector("a")?.GetAttributeValue("href", null), config.Product.ProductSkuSelector)
                        : product.QuerySelector(config.Product.ProductSkuSelector)?.InnerText?.Trim();

                    items.Add(new Product
                    {
                        Sku = sku ?? "",
                        Title = title,
                        Price = Clean(price),
                        CategoryUrl = categoryUrl
                    });
                }

                var nextPageUrl = doc.DocumentNode.QuerySelector(config.Category.NextPageSelector)?.GetAttributeValue("href", null);
                if (string.IsNullOrWhiteSpace(nextPageUrl))
                    break;

                currentPageUrl = nextPageUrl;
            }
        }

        // Deleting duplicates
        var dedup = items
            .GroupBy(x => string.IsNullOrWhiteSpace(x.Sku) ? $"NO-SKU|{x.Title}|{x.Price}" : $"SKU|{x.Sku}")
            .Select(g => g.First())
            .ToList();

        // Initialize Id for each product
        foreach (var product in dedup)
        {
            if (string.IsNullOrEmpty(product.Id))
            {
                product.Id = ObjectId.GenerateNewId().ToString();
            }
        }

        // Save to MongoDB

        await _productRepository.CreateManyAsync(dedup);
        // Also save to CSV for backup
        SaveCsv("test5.csv", dedup);
    }

    private string? GetSkuFromProduct(string? productUrl, string productSkuSelector)
    {
        if (productUrl is null)
            return null;

        var productPage = Load(productUrl);
        return productPage.DocumentNode.QuerySelector(productSkuSelector)?.InnerText?.Trim();
    }

    private HtmlDocument Load(string url)
    {
        var html = _http.GetStringAsync(url).Result;
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        return doc;
    }

    private static void SaveCsv(string path, IEnumerable<Product> rows)
    {
        using var sw = new StreamWriter(path);
        sw.WriteLine("SKU,Title,Price,CategoryUrl");
        foreach (var r in rows)
        {
            string q(string s) => "\"" + (s ?? "").Replace("\"", "\"\"") + "\"";
            sw.WriteLine($"{q(r.Sku)},{q(r.Title)},{q(r.Price)},{q(r.CategoryUrl)}");
        }
    }

    private static string Clean(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return "";
        var decoded = WebUtility.HtmlDecode(s);
        decoded = decoded.Replace('\u00A0', ' ').Trim();
        return decoded;
    }
}