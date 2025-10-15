using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WebScraper.Models;

public class Product
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string Sku { get; set; }
    public string Title { get; set; }
    public decimal Price { get; set; }
    public string ProductPageUrl { get; set; }
}
