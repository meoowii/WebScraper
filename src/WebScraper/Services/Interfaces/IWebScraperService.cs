using WebScraper.Models;

namespace WebScraper.Services.Interfaces;

public interface IWebScraperService
{
    Task<List<Product>> Scrap(string url, ScrapConfiguration scrapConfiguration = null);
}
