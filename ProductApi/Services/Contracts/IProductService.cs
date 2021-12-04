using ProductsApi.Structures.Models;

namespace ProductApi.Services.Contracts;

public interface IProductService
{
    Task<IEnumerable<Product>> GetAllProducts(int page = 1, int limit = 20);

    Task<Product> GetProductById(string productId);

    Task<Product> CreateProduct(Product product);
}