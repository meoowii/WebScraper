using WebScraper.Models;

namespace WebScraper.Services.Interfaces;

internal interface IWebScraperService
{
    void Scrap(string url, ScrapConfiguration scrapConfiguration = null);
}
