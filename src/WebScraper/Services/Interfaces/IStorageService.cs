using WebScraper.Models;

namespace WebScraper.Services.Interfaces;

public interface IStorageService
{
    Task StoreAsync(IEnumerable<Product> products, StorageType storageType);
}