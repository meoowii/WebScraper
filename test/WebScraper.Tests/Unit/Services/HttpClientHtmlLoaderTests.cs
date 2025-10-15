using FluentAssertions;
using Moq;
using Moq.Protected;
using System.Net;
using WebScraper.Services;

namespace WebScraper.Tests.Unit.Services
{
    public class HttpClientHtmlLoaderTests
    {
        [Fact]
        public async Task Load_ValidHtml_ReturnsParsedHtmlDocument()
        {
            // Arrange
            var html = "<html><body><p>Hello</p></body></html>";

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(html)
                });

            var httpClient = new HttpClient(handlerMock.Object);
            var loader = new HtmlDocumentService(httpClient);

            // Act
            var doc = loader.GetHtml("https://example.com");

            // Assert
            doc.Should().NotBeNull();
            doc.DocumentNode.InnerText.Should().Contain("Hello");
        }
    }
}
