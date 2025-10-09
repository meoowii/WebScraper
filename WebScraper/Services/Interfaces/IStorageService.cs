using WebScraper.Models;

namespace WebScraper.Services.Interfaces;

internal interface IStorageService
{
    Task StoreAsync(IEnumerable<Product> products, StorageType storageType);
}