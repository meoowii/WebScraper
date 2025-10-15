namespace WebScraper.Models;

public class ScrapConfiguration
{
    public bool ScrapProductPage { get; set; } // jesli true to wchodze w srodek strony
    public CategorySelectors Category { get; set; }
    public ProductSelectors Product { get; set; }
    // public DefaultCurrency { get; set; } // to moze byc w przyszlosci przydatne

    //TODO add probably two classes one which will contains configuration for selectors for categories
    //TODO add second class which will contains configuration for selectors for products   
}

public class CategorySelectors
{
    public string CategorySelector { get; set; }
    public string SubcategorySelector { get; set; }
    public string NextPageSelector { get; set; }
}

public class ProductSelectors
{
    public string ProductPageUrlSelector { get; set; }
    public string ProductContainerSelector { get; set; }
    public string ProductTitleSelector { get; set; } 
    public string ProductPriceSelector { get; set; } 
    public string ProductSkuSelector { get; set; }
    public string? PriceRegex { get; set; }
}
