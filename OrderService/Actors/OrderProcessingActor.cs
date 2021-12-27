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
    private const string OrderStatusStateName = "OrderStatus";
    
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
        var orderState = new OrderState
        {
            Id = OrderId,
            Date = DateTime.UtcNow,
            Status = OrderStatus.Submitted,
            Items = cartItems.ToList()
        };

        await StateManager.SetStateAsync(OrderDetailsStateName, orderState);
        await StateManager.SetStateAsync(OrderStatusStateName, OrderStatus.Submitted);
        
        await RegisterReminderAsync(
            PaymentFailedReminder,
            null,
            TimeSpan.FromSeconds(30),
            TimeSpan.FromMilliseconds(-1) /*disable periodic invocations*/);

        await PublishAsync(new OrderSubmittedEvent(
            orderState.Id,
            OrderStatus.Submitted.Name)
        );
    }

    public async Task NotifyPaymentSucceededAsync()
    {
        var statusChanged = await TryUpdateOrderStatusAsync(
            OrderStatus.Submitted, OrderStatus.Paid);
        
        if (statusChanged)
        {
            // Simulate a work time by setting a reminder.
            await RegisterReminderAsync(
                PaymentSucceededReminder,
                null,
                TimeSpan.FromSeconds(10),
                TimeSpan.FromMilliseconds(-1));
        }
    }

    public async Task NotifyPaymentFailedAsync()
    {
        var statusChanged = await TryUpdateOrderStatusAsync(
            OrderStatus.Submitted, OrderStatus.Paid);
        
        if (statusChanged)
        {
            // Simulate a work time by setting a reminder.
            await RegisterReminderAsync(
                PaymentFailedReminder,
                null,
                TimeSpan.FromSeconds(10),
                TimeSpan.FromMilliseconds(-1));
        }
    }
    
    public async Task<bool> CancelAsync()
    {
        var orderStatus = await StateManager.TryGetStateAsync<OrderStatus>(OrderStatusStateName);
        
        if (!orderStatus.HasValue)
        {
            Logger.LogWarning("Order with Id: {OrderId} cannot be cancelled because it doesn't exist",
                OrderId);

            return false;
        }

        if ( orderStatus.Value.Id == OrderStatus.Paid.Id || orderStatus.Value.Id == OrderStatus.Shipped.Id)
        {
            Logger.LogWarning("Order with Id: {OrderId} cannot be cancelled because it's in status {Status}",
                OrderId, orderStatus.Value.Name);

            return false;
        }

        await StateManager.SetStateAsync(OrderStatusStateName, OrderStatus.Cancelled);

        var order = await StateManager.GetStateAsync<OrderState>(OrderDetailsStateName);

        await PublishAsync(new OrderCancelledEvent(
            OrderId,
            OrderStatus.Cancelled.Name,
            $"The order was cancelled by buyer.")
        );

        return true;
    }

    public async Task<bool> ShipAsync()
    {
        var statusChanged = await TryUpdateOrderStatusAsync(
            OrderStatus.Paid, OrderStatus.Shipped);
        
        if (statusChanged)
        {
            var order = await StateManager.GetStateAsync<OrderState>(OrderDetailsStateName);

            await PublishAsync(new OrderShippedEvent(
                order.Id,
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
            "Received {Actor}[{ActorId}] reminder: {Reminder}",
            nameof(OrderProcessingActor), OrderId, reminderName);

        return reminderName switch
        {
            PaymentSucceededReminder => OnPaymentSucceededSimulatedWorkDoneAsync(),
            PaymentFailedReminder => OnPaymentFailedSimulatedWorkDoneAsync(),
            _ => Task.CompletedTask 
        };
    }

    private async Task OnPaymentSucceededSimulatedWorkDoneAsync()
    {
        var order = await StateManager.GetStateAsync<OrderState>(OrderDetailsStateName);

        await PublishAsync(new OrderPaidEvent(
            OrderId,
            OrderStatus.Paid.Name,
            "The payment was performed.",
            order.Items
                .Select(orderItem => new OrderStockItem(orderItem.Id, orderItem.Quantity)))
        );
    }

    private async Task OnPaymentFailedSimulatedWorkDoneAsync()
    {
        var order = await StateManager.GetStateAsync<OrderState>(OrderDetailsStateName);

        await PublishAsync(new OrderCancelledEvent(
            OrderId,
            OrderStatus.Cancelled.Name,
            "The order was cancelled because payment failed.")
        );
    }
    
    #endregion
    
    
    #region Actor Overrided Methods

    private int? _preMethodOrderStatusId;
    
    protected override Task OnActivateAsync()
    {
        // Provides opportunity to perform some optional setup.
        Logger.LogInformation("Activating actor id: {Id}", Id);
        
        return Task.CompletedTask;
    }
    
    protected override Task OnDeactivateAsync()
    {
        // Provides Opportunity to perform optional cleanup.
        Logger.LogInformation("Deactivating actor id: {Id}", Id);
        
        return Task.CompletedTask;
    }
    
    protected override async Task OnPreActorMethodAsync(ActorMethodContext actorMethodContext)
    {
        var orderStatus = await StateManager.TryGetStateAsync<OrderStatus>(OrderStatusStateName);

        _preMethodOrderStatusId = orderStatus.HasValue ? orderStatus.Value.Id : (int?)null;
    }

    protected override async Task OnPostActorMethodAsync(ActorMethodContext actorMethodContext)
    {
        var postMethodOrderStatus = await StateManager.TryGetStateAsync<OrderStatus>(OrderStatusStateName);

        if (postMethodOrderStatus.HasValue && _preMethodOrderStatusId != postMethodOrderStatus.Value.Id)
        {
            Logger.LogInformation("Order with Id: {OrderId} has been updated to status {Status}",
                OrderId, postMethodOrderStatus.Value.Name);
        }
    }
    
    #endregion

    
    #region Private Methods
    
    private async Task<bool> TryUpdateOrderStatusAsync(OrderStatus expectedOrderStatus, OrderStatus newOrderStatus)
    {
        var orderStatus = await StateManager.TryGetStateAsync<OrderStatus>(OrderStatusStateName);
        if (!orderStatus.HasValue)
        {
            Logger.LogWarning("Order with Id: {OrderId} cannot be updated because it doesn't exist",
                OrderId);

            return false;
        }

        if (orderStatus.Value.Id != expectedOrderStatus.Id)
        {
            Logger.LogWarning(
                "Order with Id: {OrderId} is in status {Status} instead of expected status {ExpectedStatus}",
                OrderId, orderStatus.Value.Name, expectedOrderStatus.Name);

            return false;
        }

        await StateManager.SetStateAsync(OrderStatusStateName, newOrderStatus);

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