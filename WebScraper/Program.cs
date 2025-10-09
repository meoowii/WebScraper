using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
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
        try
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
                    RegisterServices(services, context.Configuration);
                })
                .Build();

            await host.StartAsync();

            var scraper = host.Services.GetRequiredKeyedService<IWebScraperService>(WebScraperServiceKeys.Http);
            var storage = host.Services.GetRequiredService<IStorageService>();

            var products = await scraper.Scrap(Shops.FashionFreak);
            await storage.StoreAsync(products, StorageType.Csv | StorageType.Mongo);

            await host.StopAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private static void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        // Configure MongoDB settings
        services.Configure<MongoDbSettings>(configuration.GetSection("MongoDB"));

        // Register MongoDB client and collection
        services.AddSingleton<IMongoClient>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
            return new MongoClient(settings.ConnectionString);
        });
        services.AddSingleton(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
            var client = sp.GetRequiredService<IMongoClient>();
            return client.GetDatabase(settings.DatabaseName);
        });

        // Register repositories
        services.AddSingleton<IProductRepository, MongoProductRepository>();

        // Register existing services
        services.AddKeyedSingleton<IWebScraperService, WebScraperService>(WebScraperServiceKeys.Http);
        services.AddKeyedSingleton<IWebScraperService, WebScraperSeleniumService>(WebScraperServiceKeys.Selenium);
        services.AddSingleton<IScrapConfigurationProvider, ScrapConfigurationProvider>();
        services.AddSingleton<IStorageService, StorageService>();
    }
}