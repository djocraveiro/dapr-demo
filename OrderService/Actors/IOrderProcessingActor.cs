using Dapr.Actors;
using OrderService.Structures.Models;

namespace OrderService.Actors;

public interface IOrderProcessingActor : IActor
{
    Task SubmitAsync(IEnumerable<CartItem> cartItems);
    
    Task NotifyPaymentSucceededAsync();

    Task<bool> CancelAsync();
    
    Task<bool> ShipAsync();
    
    Task<OrderState> GetOrderDetails();
}