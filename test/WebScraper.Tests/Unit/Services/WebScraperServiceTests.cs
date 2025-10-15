using FluentAssertions;
using HtmlAgilityPack;
using NSubstitute;
using WebScraper.Models;
using WebScraper.Services;
using WebScraper.Services.Interfaces;
using WebScraper.Tests.TestAssets.Html;

namespace WebScraper.Tests.Unit.Services;

public class WebScraperServiceTests
{
    private readonly IScrapConfigurationProvider _mockConfigProvider;
    private readonly IHtmlDocumentService _mockHtmlService;
    private readonly WebScraperService _webScraperService;
    private readonly ScrapConfiguration _defaultConfig;

    public WebScraperServiceTests()
    {
        _mockConfigProvider = Substitute.For<IScrapConfigurationProvider>();
        _mockHtmlService = Substitute.For<IHtmlDocumentService>();
        _webScraperService = new WebScraperService(_mockConfigProvider, _mockHtmlService);

        _defaultConfig = new ScrapConfiguration
        {
            Category = new CategorySelectors
            {
                CategorySelector = "a.category-item",
                NextPageSelector = "a.next-page"
            },
            Product = new ProductSelectors
            {
                ProductContainerSelector = "div.card",
                ProductTitleSelector = "span.title",
                ProductPriceSelector = "span.price",
                ProductSkuSelector = "span.sku-inline",
                ProductPageUrlSelector = "a",
                PriceRegex = @"(?<price>[\s\d,\.]+)"
            },
            ScrapProductPage = false
        };
    }

    [Fact]
    public async Task Scrap_WithValidUrl_ReturnsProducts()
    {
        // Arrange
        var url = "https://example.com";
        var startPageDoc = new HtmlDocument();
        startPageDoc.LoadHtml(SampleHtml.StartPageWithTwoCategories);

        var categoryPageDoc1 = new HtmlDocument();
        categoryPageDoc1.LoadHtml(SampleHtml.OneProductCard());

        var categoryPageDoc2 = new HtmlDocument();
        categoryPageDoc2.LoadHtml(SampleHtml.OneProductCard(sku: "sku-222"));

        _mockConfigProvider.GetDefaultConfiguration(url).Returns(_defaultConfig);
        _mockHtmlService.GetHtml(url).Returns(startPageDoc);
        _mockHtmlService.GetHtml("/sukienki").Returns(categoryPageDoc1);
        _mockHtmlService.GetHtml("/spodnie").Returns(categoryPageDoc2);

        // Act
        var result = await _webScraperService.Scrap(url);

        // Assert
        result.Should().HaveCount(2); // One product from each category
        result.Should().AllSatisfy(product =>
        {
            product.Title.Should().Be("Sukienka letnia");
            product.Price.Should().Be(249.99m);
            product.Id.Should().NotBeNull();
        });
        result[0].Sku.Should().Be("sku-111");
        result[1].Sku.Should().Be("sku-222");
    }

    [Fact]
    public async Task Scrap_WithCustomConfiguration_UsesProvidedConfig()
    {
        // Arrange
        var url = "https://example.com";
        var customConfig = new ScrapConfiguration
        {
            Category = new CategorySelectors
            {
                CategorySelector = "div.custom-category",
                NextPageSelector = "a.custom-next"
            },
            Product = new ProductSelectors
            {
                ProductContainerSelector = "div.custom-card",
                ProductTitleSelector = "h1.custom-title",
                ProductPriceSelector = "span.custom-price",
                ProductSkuSelector = "span.custom-sku",
                ProductPageUrlSelector = "a.custom-link",
                PriceRegex = @"(?<price>[\s\d,\.]+)"
            }
        };

        var doc = new HtmlDocument();
        doc.LoadHtml("<html><body></body></html>");

        _mockHtmlService.GetHtml(url).Returns(doc);

        // Act
        var result = await _webScraperService.Scrap(url, customConfig);

        // Assert
        result.Should().BeEmpty();
        _mockConfigProvider.DidNotReceive().GetDefaultConfiguration(Arg.Any<string>());
    }

    [Fact]
    public async Task Scrap_WithPagination_ProcessesAllPages()
    {
        // Arrange
        var url = "https://example.com";
        var startPageDoc = new HtmlDocument();
        startPageDoc.LoadHtml(@"
            <html><body>
                <a class='category-item' href='/category'></a>
            </body></html>");

        var firstPageDoc = new HtmlDocument();
        firstPageDoc.LoadHtml($@"
            <html><body>
                {SampleHtml.OneProductCard()}
                <a class='next-page' href='/category?page=2'></a>
            </body></html>");

        var secondPageDoc = new HtmlDocument();
        secondPageDoc.LoadHtml($@"
            <html><body>
                {SampleHtml.OneProductCard(sku: "sku-222")}
            </body></html>");

        _mockConfigProvider.GetDefaultConfiguration(url).Returns(_defaultConfig);
        _mockHtmlService.GetHtml(url).Returns(startPageDoc);
        _mockHtmlService.GetHtml("/category").Returns(firstPageDoc);
        _mockHtmlService.GetHtml("/category?page=2").Returns(secondPageDoc);

        // Act
        var result = await _webScraperService.Scrap(url);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Scrap_WithProductPageScraping_LoadsDetailPages()
    {
        // Arrange
        var url = "https://example.com";
        var config = new ScrapConfiguration
        {
            Category = _defaultConfig.Category,
            Product = _defaultConfig.Product,
            ScrapProductPage = true
        };

        var startPageDoc = new HtmlDocument();
        startPageDoc.LoadHtml(@"
            <html><body>
                <a class='category-item' href='/category'></a>
            </body></html>");

        var categoryPageDoc = new HtmlDocument();
        categoryPageDoc.LoadHtml(@"
            <html><body>
                <div class='card'>
                    <a href='/product/1'>Product</a>
                </div>
            </body></html>");

        var productPageDoc = new HtmlDocument();
        productPageDoc.LoadHtml(@"
            <html><body>
                <span class='title'>Detailed Product</span>
                <span class='price'>299,99 PLN</span>
                <span class='sku-inline'>SKU123</span>
            </body></html>");

        _mockConfigProvider.GetDefaultConfiguration(url).Returns(config);
        _mockHtmlService.GetHtml(url).Returns(startPageDoc);
        _mockHtmlService.GetHtml("/category").Returns(categoryPageDoc);
        _mockHtmlService.GetHtml("/product/1").Returns(productPageDoc);

        // Act
        var result = await _webScraperService.Scrap(url);

        // Assert
        result.Should().ContainSingle();
        var product = result[0];
        product.Title.Should().Be("Detailed Product");
        product.Price.Should().Be(299.99m);
        product.Sku.Should().Be("SKU123");
    }

    [Theory]
    [InlineData("99,99 PLN", 99.99)]
    [InlineData("79.90 EUR", 79.90)]
    [InlineData("1 299,00 USD", 1299.00)]
    [InlineData("Invalid Price", 0)]
    [InlineData("", 0)]
    [InlineData(null, 0)]
    public async Task Scrap_WithDifferentPriceFormats_ParsesCorrectly(
        string priceText, decimal expectedPrice)
    {
        // Arrange
        var url = "https://example.com";
        var startPageDoc = new HtmlDocument();
        startPageDoc.LoadHtml(@"
            <html><body>
                <a class='category-item' href='/category'></a>
            </body></html>");

        var categoryPageDoc = new HtmlDocument();
        categoryPageDoc.LoadHtml($@"
            <html><body>
                <div class='card'>
                    <span class='title'>Test Product</span>
                    <span class='price'>{priceText}</span>
                </div>
            </body></html>");

        _mockConfigProvider.GetDefaultConfiguration(url).Returns(_defaultConfig);
        _mockHtmlService.GetHtml(url).Returns(startPageDoc);
        _mockHtmlService.GetHtml("/category").Returns(categoryPageDoc);

        // Act
        var result = await _webScraperService.Scrap(url);

        // Assert
        result.Should().ContainSingle();
        var product = result[0];
        product.Price.Should().Be(expectedPrice);
    }
}