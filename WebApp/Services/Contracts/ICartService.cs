using WebApp.Models;
using System.Threading.Tasks;

namespace WebApp.Services.Contracts;

public interface ICartService
{
    Task<int> AddToCart(Product product);
    
    Task<IEnumerable<CartItem>> GetCartItems();

    Task SubmitCart(IEnumerable<CartItem> items);
}