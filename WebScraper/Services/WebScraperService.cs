using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using System.Net;
using WebScraper.Models;
using WebScraper.Services.Interfaces;
using MongoDB.Bson;
using System.Globalization;
using System.Text.RegularExpressions;

namespace WebScraper.Services;

class WebScraperService : IWebScraperService
{
    private readonly IScrapConfigurationProvider _scrapConfiguration;
    private readonly IProductRepository _productRepository;
    private readonly HttpClient _http = new HttpClient();
    private readonly IStorageService _storageService;


    public WebScraperService(
        IScrapConfigurationProvider scrapConfiguration,
        IProductRepository productRepository, IStorageService storageService)
    {
        _scrapConfiguration = scrapConfiguration;
        _productRepository = productRepository;
        _storageService = storageService;
    }

    public async Task<List<Product>> Scrap(string url, ScrapConfiguration scrapConfiguration = null)
    {
        //TODO: Log start scraping + url sklepu
        var config = scrapConfiguration ?? _scrapConfiguration.GetDefaultConfiguration(url);

        // Taking all category links
        var start = Load(url);
        var categoryLinks = start.DocumentNode
            .QuerySelectorAll(config.Category.CategorySelector)
            .Select(a => a.GetAttributeValue("href", null))
            .Where(h => !string.IsNullOrWhiteSpace(h))
            .Distinct()
            .ToList();
        // TODO: Dodac logi ile kategorii zostalo zczytanych
        var items = new List<Product>();
        //TODO: parallel foreach, concurentbag 
        foreach (var categoryUrl in categoryLinks)
        {
            var currentPageUrl = categoryUrl;
            // go through pagination until no next-page
            while (true)
            {
                var doc = Load(currentPageUrl);

                var products = doc.DocumentNode.QuerySelectorAll(config.Product.ProductContainerSelector);
                foreach (var product in products)
                {
                    var title = product.QuerySelector(config.Product.ProductTitleSelector)?.InnerText?.Trim() ?? "";
                    //var price = Convert.ToDecimal(product.QuerySelector(config.Product.ProductPriceSelector)?.InnerText?.Trim() ?? "");
                    var rawPrice = product.QuerySelector(config.Product.ProductPriceSelector)?.InnerText ?? "";
                    decimal price;
                    string currency;
                    ParsePriceAndCurrency(rawPrice, out price, out currency, config.Product.PriceRegex);
                    var productPageUrl = product.QuerySelector("a")?.GetAttributeValue("href", null) ?? "";

                    var sku = config.ScrapProductPage
                        ? GetSkuFromProduct(product.QuerySelector("a")?.GetAttributeValue("href", null), config.Product.ProductSkuSelector)
                        : product.QuerySelector(config.Product.ProductSkuSelector)?.InnerText?.Trim();

                    items.Add(new Product
                    {
                        Sku = sku ?? "",
                        Title = title,
                        Price = price,
                        Currency = currency,
                        ProductPageUrl = productPageUrl
                    });
                }

                var nextPageUrl = doc.DocumentNode.QuerySelector(config.Category.NextPageSelector)?.GetAttributeValue("href", null);
                if (string.IsNullOrWhiteSpace(nextPageUrl))
                    break;

                currentPageUrl = nextPageUrl;
            }
        }

        // Deleting duplicates
        var distinctProducts = items
            .GroupBy(x => string.IsNullOrWhiteSpace(x.Sku) ? $"NO-SKU|{x.Title}|{x.Price}" : $"SKU|{x.Sku}")
            .Select(g => g.First())
            .ToList();

        // Initialize Id for each product
        foreach (var product in distinctProducts)
        {
            if (string.IsNullOrEmpty(product.Id))
            {
                product.Id = ObjectId.GenerateNewId().ToString();
            }
        }
        //TODO: Scraper zakonczyl scrapowanie dla sklepu x i zescrapowano y produktow w z czasie
        return distinctProducts;
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

    private static void ParsePriceAndCurrency(string? rawPrice, out decimal amount, out string currency, string? pattern = null)
    {
        amount = 0m;
        currency = "";

        if (string.IsNullOrWhiteSpace(rawPrice))
            return;

        var decoded = WebUtility.HtmlDecode(rawPrice).Replace('\u00A0', ' ').Trim();

        pattern ??= @"(?<amount>[\d.,]+)\s?(?<currency>[A-Za-zżźćńółęąśŻŹĆĄŚĘŁÓŃ]+)?";

        var match = Regex.Match(decoded, pattern);
        if (!match.Success)
            return;

        var amountText = match.Groups["amount"].Value.Replace(",", ".").Trim();
        currency = match.Groups["currency"].Value.Trim();

        decimal.TryParse(amountText, NumberStyles.Any, CultureInfo.InvariantCulture, out amount);
    }
}