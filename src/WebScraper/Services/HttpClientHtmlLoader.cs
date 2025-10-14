using HtmlAgilityPack;
using WebScraper.Services.Interfaces;

namespace WebScraper.Services;

public class HttpClientHtmlLoader : IHtmlLoader
{
    private readonly HttpClient _http;

    public HttpClientHtmlLoader(HttpClient httpClient)
    {
        _http = httpClient;
    }

    public HtmlDocument Load(string url)
    {
        var html = _http.GetStringAsync(url).Result;

        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        return doc;
    }
}

