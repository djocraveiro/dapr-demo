using OrderService.Structures.Models;

namespace OrderService.Actors;

public class OrderState
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.New;
    public List<CartItem> Items { get; set; } = new();

    public double GetTotal() => Items.Sum(o => o.Quantity * o.Price);
}