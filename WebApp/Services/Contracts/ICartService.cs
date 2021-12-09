using WebApp.Models;
using System.Threading.Tasks;

public interface ICartService
{
    Task<int> AddToCart(Product product);
}