using WebScraper.Models;
using WebScraper.Services.Interfaces;

namespace WebScraper.Services;

internal class WebScraperSeleniumService : IWebScraperService
{
    public Task Scrap(string url, ScrapConfiguration scrapConfiguration = null)
    {
        throw new NotImplementedException();
    }
}