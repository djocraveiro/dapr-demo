using System.Text;
using System.Text.Json;
using Dapr.Actors.Runtime;
using OrderService.Structures.Events;
using OrderService.Structures.Models;

namespace OrderService.Actors;

public class OrderProcessingActor : Actor, IOrderProcessingActor, IRemindable
{
    #region Properties
    
    private const string OrderDetailsStateName = "OrderDetails";

    private const string PaymentSucceededReminder = "PaymentSucceeded";
    private const string PaymentFailedReminder = "PaymentFailed";

    private readonly IHttpClientFactory _clientFactory;
    private HttpClient DaprClient => _clientFactory.CreateClient("dapr");
    private readonly string _pubsubName;

    private Guid OrderId => Guid.Parse(Id.GetId());

    #endregion

    
    #region Constructors

    public OrderProcessingActor(
        ActorHost host,
        IHttpClientFactory clientFactory)
        : base(host)
    {
        _clientFactory = clientFactory;
        
        var cartEventBus = Environment.GetEnvironmentVariable("CART_EVENT_BUS");
        var parts = cartEventBus?.Split('/');
        _pubsubName = parts?.First();
    }
    
    #endregion

    
    #region IOrderProcessingActor implementation

    public async Task SubmitAsync(IEnumerable<CartItem> cartItems)
    {
        var order = new OrderState
        {
            Id = OrderId,
            Date = DateTime.UtcNow,
            Status = OrderStatus.Submitted,
            Items = cartItems.ToList()
        };

        await StateManager.SetStateAsync(OrderDetailsStateName, order);

        await RegisterReminderAsync(
            PaymentFailedReminder,
            null,
            TimeSpan.FromSeconds(30),
            TimeSpan.FromMilliseconds(-1) /*disable periodic invocations*/);
    }

    public async Task NotifyPaymentSucceededAsync()
    {
        var statusChanged = await TryUpdateOrderStateAsync(
            OrderStatus.Submitted, OrderStatus.Paid);
        
        if (statusChanged)
        {
            // Simulate a work time by setting a reminder.
            await RegisterReminderAsync(
                PaymentSucceededReminder,
                null,
                TimeSpan.FromSeconds(5),
                TimeSpan.FromMilliseconds(-1));
        }
    }

    public async Task<bool> CancelAsync()
    {
        var order = await StateManager.TryGetStateAsync<OrderState>(OrderDetailsStateName);

        if (!order.HasValue)
        {
            Logger.LogWarning($"Order with Id: {OrderId} cannot be cancelled because it doesn't exist");

            return false;
        }

        var orderStatus = order.Value.Status;
        if ( orderStatus.Id == OrderStatus.Paid.Id || orderStatus.Id == OrderStatus.Shipped.Id)
        {
            Logger.LogWarning(
                $"Order with Id: {OrderId} cannot be cancelled because it's in status {orderStatus.Name}");

            return false;
        }

        order.Value.Status = OrderStatus.Cancelled;
        await StateManager.SetStateAsync(OrderDetailsStateName, order.Value);
        
        await PublishAsync(new OrderCancelledEvent(
            OrderId,
            OrderStatus.Cancelled.Name,
            $"The order was cancelled by buyer.")
        );

        return true;
    }

    public async Task<bool> ShipAsync()
    {
        var statusChanged = await TryUpdateOrderStateAsync(
            OrderStatus.Paid, OrderStatus.Shipped);
        
        if (statusChanged)
        {
            await PublishAsync(new OrderShippedEvent(
                OrderId,
                OrderStatus.Shipped.Name,
                "The order was shipped.")
            );

            return true;
        }

        return false;
    }

    public Task<OrderState> GetOrderDetails()
    {
        return StateManager.GetStateAsync<OrderState>(OrderDetailsStateName); 
    }

    #endregion

    
    #region IRemindable implementation
    
    public Task ReceiveReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period)
    {
        Logger.LogInformation(
            $"Received {nameof(OrderProcessingActor)}[{OrderId}] reminder: {reminderName}");

        return reminderName switch
        {
            PaymentSucceededReminder => OnPaymentSucceededSimulatedWorkDoneAsync(),
            PaymentFailedReminder => OnPaymentFailedSimulatedWorkDoneAsync(),
            _ => Task.CompletedTask 
        };
    }

    private async Task OnPaymentSucceededSimulatedWorkDoneAsync()
    {
        var statusChanged = await TryUpdateOrderStateAsync(
            OrderStatus.Paid, OrderStatus.Shipped);

        if (statusChanged)
        {
            await PublishAsync(new OrderShippedEvent(
                OrderId,
                OrderStatus.Shipped.Name,
                "The order was shipped.")
            );
        }
    }

    private async Task OnPaymentFailedSimulatedWorkDoneAsync()
    {
        var statusChanged = await TryUpdateOrderStateAsync(
            OrderStatus.Submitted, OrderStatus.Cancelled);

        if (statusChanged)
        {
            await PublishAsync(new OrderCancelledEvent(
                OrderId,
                OrderStatus.Cancelled.Name,
                "The order was cancelled because payment failed.")
            );
        }
    }
    
    #endregion
    
    
    #region Actor Overrided Methods

    private int? _preMethodOrderStatusId;
    
    protected override Task OnActivateAsync()
    {
        // Provides opportunity to perform some optional setup.
        Logger.LogInformation($"Activating actor id: {Id.GetId()}");
        
        return Task.CompletedTask;
    }
    
    protected override Task OnDeactivateAsync()
    {
        // Provides Opportunity to perform optional cleanup.
        Logger.LogInformation($"Deactivating actor id: {Id.GetId()}");
        
        return Task.CompletedTask;
    }
    
    protected override async Task OnPreActorMethodAsync(ActorMethodContext actorMethodContext)
    {
        var order = await StateManager.TryGetStateAsync<OrderState>(OrderDetailsStateName);

        _preMethodOrderStatusId = order.HasValue ? order.Value.Status.Id : null;
    }

    protected override async Task OnPostActorMethodAsync(ActorMethodContext actorMethodContext)
    {
        var order = await StateManager.TryGetStateAsync<OrderState>(OrderDetailsStateName);

        if (order.HasValue && _preMethodOrderStatusId != order.Value.Status.Id)
        {
            Logger.LogInformation(
                $"Order with Id: {OrderId} has been updated to status {order.Value.Status.Name}");
        }
    }
    
    #endregion

    
    #region Private Methods
    
    private async Task<bool> TryUpdateOrderStateAsync(OrderStatus expectedStatus, OrderStatus newStatus)
    {
        var order = await StateManager.TryGetStateAsync<OrderState>(OrderDetailsStateName);
        if (!order.HasValue)
        {
            Logger.LogWarning($"Order with Id: {OrderId} cannot be updated because it doesn't exist");

            return false;
        }

        var orderStatus = order.Value.Status;
        if (orderStatus.Id != expectedStatus.Id)
        {
            Logger.LogWarning($"Order with Id: {OrderId} " +
                $"is in status {orderStatus.Name} instead of expected status {expectedStatus.Name}");

            return false;
        }

        order.Value.Status = newStatus;
        await StateManager.SetStateAsync(OrderDetailsStateName, order.Value);

        return true;
    }
    
    private async Task PublishAsync(BaseEvent @event)
    {
        var topicName = @event.GetType().Name;
        
        var payload = JsonSerializer.Serialize(@event);
        var content = new StringContent(payload, Encoding.UTF8, "application/json");

        await DaprClient.PostAsync($"v1.0/publish/{_pubsubName}/{topicName}", content);
    }
    
    #endregion
}