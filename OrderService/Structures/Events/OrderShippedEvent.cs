namespace OrderService.Structures.Events;

public record OrderShippedEvent(
        Guid OrderId,
        string OrderStatus,
        string Description)
    : BaseEvent;