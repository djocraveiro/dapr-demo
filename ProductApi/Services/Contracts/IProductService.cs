using ProductApi.Structures.Models;

namespace ProductApi.Services.Contracts;

public interface IProductService
{
    Task<IEnumerable<Product>> GetAllProducts(int page = 1, int limit = 20, double? minPrice = null,
        double? maxPrice = null);

    Task<Product> GetProductById(string productId);

    Task<Product> CreateProduct(Product product);
}