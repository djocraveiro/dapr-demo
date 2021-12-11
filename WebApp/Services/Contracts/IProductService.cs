using WebApp.Models;

namespace WebApp.Services.Contracts;

public interface IProductService
{
    Task<IEnumerable<Product>> GetProducts(int page, int limit);
}