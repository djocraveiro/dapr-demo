namespace OrderService.Structures.Events;

public record OrderPaymentFailedEvent(
        Guid OrderId)
    : BaseEvent;