using WebScraper.Constants;
using WebScraper.Models;
using WebScraper.Services.Interfaces;

namespace WebScraper.Services;

class ScrapConfigurationProvider : IScrapConfigurationProvider
{
    private readonly Dictionary<string, ScrapConfiguration> _configurations;
    public ScrapConfigurationProvider()
    {
        _configurations = new Dictionary<string, ScrapConfiguration>
        {
            { Shops.FashionFreak, GetFashionFreakConfig() }
        };
    }

    public ScrapConfiguration GetDefaultConfiguration(string url)
    {
        return _configurations.FirstOrDefault(c => url.Contains(c.Key)).Value;
    }

    private static ScrapConfiguration GetFashionFreakConfig()
    {
        var categorySelectors = new CategorySelectors
        {
            CategorySelector = "ul.wc-block-product-categories-list li a",
            SubcategorySelector = "",
            NextPageSelector = "a.next.page-numbers"
        };
        var productSelectors = new ProductSelectors
        {
            ProductContainerSelector = "ul.products li.product",
            ProductTitleSelector = "h2.woocommerce-loop-product__title",
            ProductPriceSelector = "span.woocommerce-Price-amount",
            ProductSkuSelector = "span.sku",
            PriceRegex = @"(?<amount>[\d.,]+)\s?(?<currency>[A-Za-zżźćńółęąśŻŹĆĄŚĘŁÓŃ]+)"
        };
        return new ScrapConfiguration
        { 
            ScrapProductPage = false,
            Category = categorySelectors,
            Product = productSelectors
        };
    }
}