using HtmlAgilityPack;

namespace WebScraper.Services.Interfaces;

public interface IHtmlDocumentService
{
    HtmlDocument GetHtml(string url);
}

