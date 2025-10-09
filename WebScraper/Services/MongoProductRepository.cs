using MongoDB.Driver;
using WebScraper.Models;
using WebScraper.Services.Interfaces;

namespace WebScraper.Services;

public class MongoProductRepository : IProductRepository
{
    private readonly IMongoCollection<Product> _productsCollection;

    public MongoProductRepository(IMongoDatabase mongoDatabase)
    {
        _productsCollection = mongoDatabase.GetCollection<Product>("products");
    }

    public async Task CreateManyAsync(IEnumerable<Product> products)
    {
        await _productsCollection.InsertManyAsync(products);
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _productsCollection.Find(_ => true).ToListAsync();
    }

    public async Task<Product?> GetBySkuAsync(string sku)
    {
        return await _productsCollection.Find(x => x.Sku == sku).FirstOrDefaultAsync();
    }
}