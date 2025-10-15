using WebScraper.Models;

namespace WebScraper.Services.Interfaces;

public interface IScrapConfigurationProvider
{
    ScrapConfiguration GetDefaultConfiguration(string url);
}
