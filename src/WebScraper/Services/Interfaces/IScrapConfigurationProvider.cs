using WebScraper.Models;

namespace WebScraper.Services.Interfaces;

internal interface IScrapConfigurationProvider
{
    ScrapConfiguration GetDefaultConfiguration(string url);
}
