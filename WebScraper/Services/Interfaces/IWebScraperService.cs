using WebScraper.Models;

namespace WebScraper.Services.Interfaces;

internal interface IWebScraperService
{
    Task<List<Product>> Scrap(string url, ScrapConfiguration scrapConfiguration = null);
}
