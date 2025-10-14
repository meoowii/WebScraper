using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using WebScraper.Models;

namespace WebScraper.Services
{
    partial class WebScraperService
    {
        internal static List<string> ExtractCategoryLinks(HtmlDocument doc, ScrapConfiguration config)
        {
            return doc.DocumentNode
                .QuerySelectorAll(config.Category.CategorySelector)
                .Select(a => a.GetAttributeValue("href", null))
                .Where(h => !string.IsNullOrWhiteSpace(h))
                .Distinct()
                .ToList();
        }

        internal static List<Product> ExtractProductsFromPage(HtmlDocument doc, ScrapConfiguration config)
        {
            var items = new List<Product>();
            var products = doc.DocumentNode.QuerySelectorAll(config.Product.ProductContainerSelector);

            foreach (var product in products)
            {
                var title = product.QuerySelector(config.Product.ProductTitleSelector)?.InnerText?.Trim() ?? "";
                var rawPrice = product.QuerySelector(config.Product.ProductPriceSelector)?.InnerText ?? "";

                ParsePriceAndCurrency(rawPrice, out var price, out var currency, config.Product.PriceRegex);

                var productPageUrl = product.QuerySelector("a")?.GetAttributeValue("href", null) ?? "";
                var skuInline = product.QuerySelector(config.Product.ProductSkuSelector)?.InnerText?.Trim();

                items.Add(new Product
                {
                    Sku = skuInline ?? "",
                    Title = title,
                    Price = price,
                    Currency = currency,
                    ProductPageUrl = productPageUrl
                });
            }
            return items;
        }

        internal static List<Product> RemoveDuplicates(List<Product> items)
        {
            return items
                .GroupBy(x => string.IsNullOrWhiteSpace(x.Sku)
                    ? $"NO-SKU|{x.Title}|{x.Price}"
                    : $"SKU|{x.Sku}")
                .Select(g => g.First())
                .ToList();
        }

        internal static void InitializeId(List<Product> products)
        {
            foreach (var p in products)
            {
                if (string.IsNullOrEmpty(p.Id))
                    p.Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
            }
        }
    }
}
