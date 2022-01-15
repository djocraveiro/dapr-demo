using MongoDB.Bson;
using MongoDB.Driver;
using ProductApi.Extensions;
using ProductApi.Services.Contracts;
using ProductApi.Structures.Documents;
using ProductApi.Structures.Models;

namespace ProductApi.Services;

public class ProductService : IProductService
{
    private readonly IMongoDatabase _database;

    private IMongoCollection<ProductDocument> ProductCollection => _database.GetCollection<ProductDocument>("products");


    public ProductService(IMongoClient mongo)
    {
        _database = mongo.GetDatabase("app_db");
    }


    public async Task<IEnumerable<Product>> GetAllProducts(int page = 1, int limit = 20, double? minPrice = null,
        double? maxPrice = null)
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

        var filter = BuildFilterDefinition(minPrice, maxPrice);

        var products = await ProductCollection.Find(filter)
            .SortByDescending(x => x.Price)
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
    
    
    private static FilterDefinition<ProductDocument> BuildFilterDefinition(double? minPrice = null,
        double? maxPrice = null)
    {
        if (minPrice is < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(minPrice));
        }

        if (minPrice > maxPrice)
        {
            throw new ArgumentException($"{nameof(minPrice)} must be equal or lower to {nameof(maxPrice)}");
        }

        var builder = Builders<ProductDocument>.Filter;
        var filter = FilterDefinition<ProductDocument>.Empty;
        
        if (minPrice.HasValue)
        {
            filter = builder.Gte(x => x.Price, minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            filter = builder.And(filter, builder.Lte(x => x.Price, maxPrice.Value));
        }

        return filter;
    }
}