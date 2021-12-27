namespace OrderService.Structures.Events;

public record OrderSubmittedEvent(
        Guid OrderId,
        string OrderStatus)
    : BaseEvent;