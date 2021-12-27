namespace OrderService.Structures.Events;

public record OrderPaidEvent(
        Guid OrderId,
        string OrderStatus,
        string Description,
        IEnumerable<OrderStockItem> OrderStockItems)
    : BaseEvent;