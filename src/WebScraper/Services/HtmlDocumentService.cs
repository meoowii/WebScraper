using HtmlAgilityPack;
using WebScraper.Services.Interfaces;

namespace WebScraper.Services;

internal class HtmlDocumentService : IHtmlDocumentService
{
    private readonly HttpClient _http;

    public HtmlDocumentService(HttpClient httpClient)
    {
        _http = httpClient;
    }

    public HtmlDocument GetHtml(string url)
    {
        var html = _http.GetStringAsync(url).Result;

        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        return doc;
    }
}

