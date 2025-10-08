using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using WebScraper.Constants;
using WebScraper.Models;
using WebScraper.Services;
using WebScraper.Services.Interfaces;

namespace WebScraper;

class Program
{
    static async Task Main()
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            })
            .UseSerilog((context, services, configuration) =>
            {
                configuration.ReadFrom.Configuration(context.Configuration)
                             .ReadFrom.Services(services);
            })
            .ConfigureServices((context, services) =>
            {
                RegisterServices(services);
            })
            .Build();

        await host.StartAsync();
        //change to scrape with selenium
        var scraper = host.Services.GetRequiredKeyedService<IWebScraperService>(WebScraperServiceKeys.Http);

        scraper.Scrap(Shops.FashionFreak);

        await host.StopAsync();
    }

    private static void RegisterServices(IServiceCollection services)
    {
        //TODO you can do changes only here - you need to add repository to save data
        //service registration 
        services.AddKeyedSingleton<IWebScraperService, WebScraperService>(WebScraperServiceKeys.Http);        
        services.AddKeyedSingleton<IWebScraperService, WebScraperSeleniumService>(WebScraperServiceKeys.Selenium);
        services.AddSingleton<IScrapConfigurationProvider, ScrapConfigurationProvider>();
    }
}
