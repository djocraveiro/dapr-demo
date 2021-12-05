using System.Collections.Generic;
using System.Threading.Tasks;
using WebApp.Models;

namespace WebApp.Services;

public interface IProductService
{
    Task<IEnumerable<Product>> GetProducts(int page, int limit);
}