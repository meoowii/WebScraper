using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using WebScraper.Constants;
using WebScraper.Models;
using WebScraper.Services;
using WebScraper.Services.Interfaces;
using MongoDB.Driver;

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

            scraper.Scrap(Shops.FashionFreak);

            await host.StopAsync();
        }
        catch (Exception ex)
        {

            throw;
        }
    }

    private static void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        // Configure MongoDB settings
        services.Configure<MongoDbSettings>(configuration.GetSection("MongoDB"));
        var mongoSettings = configuration.GetSection("MongoDB").Get<MongoDbSettings>();

        // Register MongoDB client and collection
        services.AddSingleton<IMongoClient>(sp => new MongoClient(mongoSettings.ConnectionString));
        services.AddSingleton(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            var database = client.GetDatabase(mongoSettings.DatabaseName);
            return database.GetCollection<Product>(mongoSettings.ProductsCollectionName);
        });

        // Register repositories
        services.AddSingleton<IProductRepository, MongoProductRepository>();

        // Register existing services
        services.AddKeyedSingleton<IWebScraperService, WebScraperService>(WebScraperServiceKeys.Http);        
        services.AddKeyedSingleton<IWebScraperService, WebScraperSeleniumService>(WebScraperServiceKeys.Selenium);
        services.AddSingleton<IScrapConfigurationProvider, ScrapConfigurationProvider>();
    }
}
