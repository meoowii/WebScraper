using HtmlAgilityPack;

namespace WebScraper.Services.Interfaces;

internal interface IHtmlLoader
{
    HtmlDocument Load(string url);
}

