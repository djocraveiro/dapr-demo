using MongoDB.Bson;
using MongoDB.Driver;
using ProductApi.Extensions;
using ProductApi.Services.Contracts;
using ProductsApi.Structures.Documents;
using ProductsApi.Structures.Models;

namespace ProductsApi.Services;

public class ProductService : IProductService
{
    private readonly IMongoDatabase _database;

    private IMongoCollection<ProductDocument> ProductCollection => _database.GetCollection<ProductDocument>("products");


    public ProductService(IMongoClient mongo)
    {
        _database = mongo.GetDatabase("app_db");
    }


    public async Task<IEnumerable<Product>> GetAllProducts(int page = 1, int limit = 20)
    {
        if (page < 1)
        {
            throw new ArgumentException($"invalid {nameof(page)}");
        }

        if (limit < 1 || limit > 100)
        {
            throw new ArgumentOutOfRangeException($"{limit} must be between 1 and 100");
        }

        int skip = (page - 1) * limit;

        var products = await ProductCollection.Find(new BsonDocument())
            .Skip(skip)
            .Limit(limit)
            .ToListAsync();

        return products.Select(p => p.ToProductModel()).ToList();
    }

    public async Task<Product> GetProductById(string productId)
    {
        if (string.IsNullOrWhiteSpace(productId))
        {
            throw new ArgumentException($"invalid {nameof(productId)}");
        }

        var filter = Builders<ProductDocument>.Filter.Eq(x => x.Id, new ObjectId(productId));
        var cursor = await ProductCollection.FindAsync(filter);
        var product = cursor.SingleOrDefault();

        return product.ToProductModel();
    }

    public async Task<Product> CreateProduct(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);

        var document = product.ToProductDocument();
        await ProductCollection.InsertOneAsync(document);

        product.Id = document.Id.ToString();
        return product;
    }
}