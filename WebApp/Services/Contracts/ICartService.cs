using WebApp.Models;

namespace WebApp.Services.Contracts;

public interface ICartService
{
    Task<int> AddToCart(Product product);

    Task<int> RemoveFromCart(string productId);
    
    Task<IEnumerable<CartItem>> GetCartItems();

    Task SubmitCart(IEnumerable<CartItem> items);

    Task ClearCartItems();
}