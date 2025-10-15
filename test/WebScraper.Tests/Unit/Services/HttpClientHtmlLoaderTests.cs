using FluentAssertions;
using System.Net;
using WebScraper.Services;

namespace WebScraper.Tests.Unit.Services;

public class HttpClientHtmlLoaderTests
{
    [Fact]
    public async Task Load_ValidHtml_ReturnsParsedHtmlDocument()
    {
        // Arrange
        var html = "<html><body><p>Hello</p></body></html>";
        var handler = new TestHttpMessageHandler(html);
        var httpClient = new HttpClient(handler);
        var loader = new HtmlDocumentService(httpClient);

        // Act
        var doc = loader.GetHtml("https://example.com");

        // Assert
        doc.Should().NotBeNull();
        doc.DocumentNode.InnerText.Should().Contain("Hello");
    }
}

internal class TestHttpMessageHandler : HttpMessageHandler
{
    private readonly string _responseContent;

    public TestHttpMessageHandler(string responseContent)
    {
        _responseContent = responseContent;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(_responseContent)
        });
    }
}