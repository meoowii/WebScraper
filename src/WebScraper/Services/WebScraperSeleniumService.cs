using WebScraper.Models;
using WebScraper.Services.Interfaces;

namespace WebScraper.Services;

internal class WebScraperSeleniumService : IWebScraperService
{
    public Task<List<Product>> Scrap(string url, ScrapConfiguration scrapConfiguration = null)
    {
        throw new NotImplementedException();
    }
}