using WebScraper.Models;

namespace WebScraper.Services.Interfaces;

public interface IProductRepository
{
    Task CreateManyAsync(IEnumerable<Product> products);
    Task<IEnumerable<Product>> GetAllAsync();
    Task<Product?> GetBySkuAsync(string sku);
}