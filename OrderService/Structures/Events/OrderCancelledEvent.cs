namespace OrderService.Structures.Events;

public record OrderCancelledEvent(
        Guid OrderId,
        string OrderStatus,
        string Description)
    : BaseEvent;