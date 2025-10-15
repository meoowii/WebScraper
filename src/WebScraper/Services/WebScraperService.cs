using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using MongoDB.Bson;
using WebScraper.Models;
using WebScraper.Services.Interfaces;

namespace WebScraper.Services;

internal class WebScraperService : IWebScraperService
{
    private readonly IScrapConfigurationProvider _scrapConfiguration;
    private readonly IHtmlDocumentService _htmlServcie;

    public WebScraperService(
        IScrapConfigurationProvider scrapConfiguration,
        IHtmlDocumentService htmlService)
    {
        _scrapConfiguration = scrapConfiguration;
        _htmlServcie = htmlService;
    }

    public async Task<List<Product>> Scrap(string url, ScrapConfiguration scrapConfiguration = null)
    {
        //TODO: Log start scraping + url sklepu
        var config = scrapConfiguration ?? _scrapConfiguration.GetDefaultConfiguration(url);

        // Taking all category links
        var startPage = _htmlServcie.GetHtml(url);
        var categoryLinks = startPage.DocumentNode
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
                var paginationPage = _htmlServcie.GetHtml(currentPageUrl);

                var products = paginationPage.DocumentNode.QuerySelectorAll(config.Product.ProductContainerSelector);
                foreach (var product in products)
                {
                    var productHtmlNode = config.ScrapProductPage
                        ? GetDocumentNode(config.Product.ProductPageUrlSelector, product)
                        : product;

                    var title = productHtmlNode.QuerySelector(config.Product.ProductTitleSelector)?.InnerText?.Trim() ?? "";
                    var rawPrice = productHtmlNode.QuerySelector(config.Product.ProductPriceSelector)?.InnerText ?? "";

                    (decimal price, string currency) = ParsePriceAndCurrency(rawPrice, config.Product.PriceRegex);

                    var productPageUrl = productHtmlNode.QuerySelector(config.Product.ProductPageUrlSelector)?.GetAttributeValue("href", null) ?? "";

                    var sku = productHtmlNode.QuerySelector(config.Product.ProductSkuSelector)?.InnerText?.Trim();

                    items.Add(new Product
                    {
                        Sku = sku ?? "",
                        Title = title,
                        Price = price,
                        ProductPageUrl = productPageUrl
                    });
                }

                var nextPageUrl = paginationPage.DocumentNode.QuerySelector(config.Category.NextPageSelector)?.GetAttributeValue("href", null);
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

    private HtmlNode GetDocumentNode(string selector, HtmlNode product)
    {
        var pageUrl = product.QuerySelector(selector)?.GetAttributeValue("href", null);

        if (pageUrl is null)
            return product;

        return _htmlServcie.GetHtml(pageUrl).DocumentNode;
    }

    private static (decimal rawPrice, string currency) ParsePriceAndCurrency(string? rawPrice, string? pattern = null)
    {
        if (string.IsNullOrWhiteSpace(rawPrice))
            return (default, string.Empty);

        var decoded = WebUtility.HtmlDecode(rawPrice).Replace('\u00A0', ' ').Trim();

        var match = Regex.Match(decoded, pattern);
        if (!match.Success)
            return (default, string.Empty); ;

        var amountText = match.Groups["price"].Value.Replace(",", ".").Replace(" ","").Trim();

        var amount = decimal.TryParse(amountText, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedAmount)
            ? parsedAmount
            : default;

        return (amount, match.Groups["currency"].Value.Trim());
    }
}