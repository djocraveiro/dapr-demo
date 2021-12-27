namespace OrderService.Structures.Events;

public record BaseEvent
{
    public Guid Id { get; }

    public DateTime CreationDate { get; }

    public BaseEvent()
    {
        Id = Guid.NewGuid();
        CreationDate = DateTime.UtcNow;
    }
}