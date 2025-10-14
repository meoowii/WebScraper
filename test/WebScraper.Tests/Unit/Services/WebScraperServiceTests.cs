using System.Linq;
using FluentAssertions;
using HtmlAgilityPack;
using WebScraper.Models;
using WebScraper.Services;
using WebScraper.Tests.TestAssets.Html;
using Xunit;

namespace WebScraper.Tests.Unit.Services;
public class WebScraperServiceTests
{
   //   CATEGORY LINK EXTRACTION

    [Fact]
    public void ExtractCategoryLinks_StartPage_ReturnsAllCategoryLinks()
    {
        // Arrange
        var doc = new HtmlDocument();
        doc.LoadHtml(SampleHtml.StartPageWithTwoCategories);
        var config = new ScrapConfiguration
        {
            Category = new CategorySelectors { CategorySelector = "a.category-item" }
        };

        // Act
        var result = WebScraperService.ExtractCategoryLinks(doc, config);

        // Assert
        result.Should().BeEquivalentTo( ["/sukienki", "/spodnie"]  );
    }

    [Fact]
    public void ExtractCategoryLinks_EmptyHtml_ReturnsEmptyList()
    {
        // Arrange
        var doc = new HtmlDocument();
        doc.LoadHtml("<html><body></body></html>");
        var config = new ScrapConfiguration
        {
            Category = new CategorySelectors { CategorySelector = "a.category-item" }
        };

        // Act
        var result = WebScraperService.ExtractCategoryLinks(doc, config);

        // Assert
        result.Should().BeEmpty();
    }

    //       PRODUCT PARSING

    [Fact]
    public void ExtractProductsFromPage_OneProductCard_ReturnsParsedProduct()
    {
        // Arrange
        var doc = new HtmlDocument();
        doc.LoadHtml(SampleHtml.OneProductCard);
        var config = new ScrapConfiguration
        {
            Product = new ProductSelectors
            {
                ProductContainerSelector = "div.card",
                ProductTitleSelector = "span.title",
                ProductPriceSelector = "span.price",
                ProductSkuSelector = "span.sku-inline",
                PriceRegex = null
            }
        };

        // Act
        var result = WebScraperService.ExtractProductsFromPage(doc, config);

        // Assert
        result.Should().HaveCount(1);
        var product = result[0];
        product.Title.Should().Be("Sukienka letnia");
        product.Price.Should().Be(249.99m);
        product.Currency.Should().Be("PLN");
        product.ProductPageUrl.Should().Be("/p/sku-111");
    }

    [Theory]
    [InlineData("99,99 PLN", 99.99, "PLN")]
    [InlineData("79,90 zł", 79.90, "zł")]
    [InlineData("899,00 EUR", 899.00, "EUR")] 
    public void ExtractProductsFromPage_DifferentPriceFormats_ParsesCorrectly(
        string rawPrice, decimal expectedPrice, string expectedCurrency)
    {
        // Arrange
        var html = $@"
        <html><body>
          <div class='card'>
            <a href='/p/sku-xyz'><span class='title'>Koszulka basic</span></a>
            <span class='price'>{rawPrice}</span>
          </div>
        </body></html>";
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var config = new ScrapConfiguration
        {
            Product = new ProductSelectors
            {
                ProductContainerSelector = "div.card",
                ProductTitleSelector = "span.title",
                ProductPriceSelector = "span.price",
                ProductSkuSelector = "span.sku-inline",
                PriceRegex = null
            }
        };

        // Act
        var items = WebScraperService.ExtractProductsFromPage(doc, config);

        // Assert
        items.Should().ContainSingle();
        items[0].Title.Should().Be("Koszulka basic");
        items[0].Price.Should().BeApproximately(expectedPrice, 0.01m);
        items[0].Currency.Should().Be(expectedCurrency);
        items[0].ProductPageUrl.Should().Be("/p/sku-xyz");
    }

    //        REMOVING DUPLICATES

    [Fact]
    public void RemoveDuplicates_SameSku_ReturnsUniqueProducts()
    {
        // Arrange
        var products = new[]
        {
            new Product { Sku = "SKU-TEE-001", Title = "Koszulka basic", Price = 79.99m },
            new Product { Sku = "SKU-TEE-001", Title = "Koszulka basic", Price = 79.99m },
            new Product { Sku = "SKU-PANTS-010", Title = "Spodnie slim", Price = 199.99m }
        }.ToList();

        // Act
        var distinct = WebScraperService.RemoveDuplicates(products);

        // Assert
        distinct.Should().HaveCount(2);
        distinct.Select(p => p.Sku).Should().BeEquivalentTo(new[] { "SKU-TEE-001", "SKU-PANTS-010" });
    }

    [Fact]
    public void RemoveDuplicates_MissingSku_UsesTitleAndPriceAsFallbackKey()
    {
        // Arrange
        var products = new[]
        {
            new Product { Sku = "", Title = "Sukienka letnia", Price = 249.99m },
            new Product { Sku = "", Title = "Sukienka letnia", Price = 249.99m },
            new Product { Sku = "", Title = "Sukienka letnia", Price = 199.99m },
             new Product { Sku = "", Title = "Sukienka zimowa", Price = 249.99m }
        }.ToList();

        // Act
        var distinct = WebScraperService.RemoveDuplicates(products);

        // Assert
        distinct.Should().HaveCount(3);
        distinct.Should().ContainSingle(p => p.Title == "Sukienka letnia" && p.Price == 249.99m);
        distinct.Should().ContainSingle(p => p.Title == "Sukienka letnia" && p.Price == 199.99m);
        distinct.Should().ContainSingle(p => p.Title == "Sukienka zimowa" && p.Price == 249.99m);
    }

    //        INITIALIZING ID

    [Fact]
    public void InitializeId_ProductsWithoutId_AssignsNewIds()
    {
        // Arrange
        var items = new[]
        {
            new Product { Id = null,     Sku = "SKU-DRS-100" },
            new Product { Id = "",       Sku = "SKU-TEE-200" },
            new Product { Id = "EXISTING-ID",  Sku = "SKU-PANTS-300" }
        }.ToList();

        // Act
        WebScraperService.InitializeId(items);

        // Assert
        items[0].Id.Should().NotBeNullOrWhiteSpace();
        items[1].Id.Should().NotBeNullOrWhiteSpace();
        items[2].Id.Should().Be("EXISTING-ID");
    }
}
