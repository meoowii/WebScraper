using System.Globalization;
using WebScraper.Models;
using WebScraper.Services.Interfaces;

namespace WebScraper.Services;

internal class StorageService : IStorageService
{
    private readonly IProductRepository _productRepository;

    public StorageService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task StoreAsync(IEnumerable<Product> products, StorageType storageType)
    {
        var list = products.ToList();

        if (storageType.HasFlag(StorageType.Mongo))
        {
            await _productRepository.CreateManyAsync(list);
            //TODO: Produkty zapisano do MongoDB w czasie xx
        }
        if (storageType.HasFlag(StorageType.Csv))
        {
            SaveCsv($"products_{DateTime.UtcNow:yyyy_MM_dd_hh_mm_ss}.csv", list);
            //TODO: Produkty zapisano do csv w czasie xx
        }
    }
    private static void SaveCsv(string path, IEnumerable<Product> rows)
    {
        using var sw = new StreamWriter(path);
        sw.WriteLine("SKU,Title,Price,Currency,ProductPageUrl");
        foreach (var r in rows)
        {
            string Q(string s) => "\"" + (s ?? "").Replace("\"", "\"\"") + "\"";
            var priceText = r.Price.ToString(CultureInfo.InvariantCulture);

            sw.WriteLine($"{Q(r.Sku)},{Q(r.Title)},{priceText},{Q(r.ProductPageUrl)}");
        }
    }
}

