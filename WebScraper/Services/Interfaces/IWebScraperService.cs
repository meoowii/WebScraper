using WebScraper.Models;

namespace WebScraper.Services.Interfaces;

internal interface IWebScraperService
{
    Task Scrap(string url, ScrapConfiguration scrapConfiguration = null);
}
