using System.Net;
using System.Text.Json;
using CloudNative.CloudEvents.AspNetCore;
using CloudNative.CloudEvents.SystemTextJson;
using Dapr.Actors;
using Dapr.Actors.Client;
using Microsoft.AspNetCore.Mvc;
using OrderService.Actors;
using OrderService.Structures.Events;
using OrderService.Structures.Models;

namespace OrderService.Controllers;

[ApiController]
[Route("[controller]")]
public class DaprController : ControllerBase
{
    #region Properties
    
    private readonly Random _random = new ();
    private readonly IActorProxyFactory _actorProxyFactory;
    private readonly ILogger<DaprController> _logger;
    private readonly (string pubsubname, string topic) _cartEventBus;

    #endregion

    
    #region Constructors

    public DaprController(IActorProxyFactory actorProxyFactory, ILogger<DaprController> logger)
    {
        _actorProxyFactory = actorProxyFactory;
        _logger = logger;
        
        var cartEventBus = Environment.GetEnvironmentVariable("CART_EVENT_BUS");
        var parts = cartEventBus?.Split('/');
        if (parts is not { Length: 2 })
        {
            throw new ArgumentException("invalid CART_EVENT_BUS");
        }

        _cartEventBus = (parts.First(), parts.Last());
    }
    
    #endregion
    

    #region Public Methods

    [HttpGet("subscribe")]
    public ActionResult Subscribe()
    {
        _logger.LogInformation("Subscription has been made.");
        
        var payload = new[]
        {
            new
            {
                pubsubname = _cartEventBus.pubsubname,
                topic= _cartEventBus.topic,
                route = "cart_checkout"
            },
            new
            {
                pubsubname = _cartEventBus.pubsubname,
                topic= nameof(OrderSubmittedEvent),
                route = "order_submitted"
            },
            new
            {
                pubsubname = _cartEventBus.pubsubname,
                topic= nameof(OrderPaidEvent),
                route = "order_paid"
            },
            new
            {
                pubsubname = _cartEventBus.pubsubname,
                topic= nameof(OrderPaymentFailedEvent),
                route = "order_payment_failed"
            },
            new
            {
                pubsubname = _cartEventBus.pubsubname,
                topic= nameof(OrderCancelledEvent),
                route = "order_cancelled"
            },
            new
            {
                pubsubname = _cartEventBus.pubsubname,
                topic= nameof(OrderShippedEvent),
                route = "order_shipped"
            }
        };
        
        return Ok(payload);
    }

    [HttpPost("/cart_checkout")]
    public async Task<ActionResult> OrderReceived()
    {
        var jsonFormatter = new JsonEventFormatter();
        var cloudEvent = await Request.ToCloudEventAsync(jsonFormatter);
        
        _logger.LogInformation("Order received.");
        _logger.LogDebug($"Cloud event {cloudEvent.Id} {cloudEvent.Type} {cloudEvent.DataContentType}");
        
        if (TrySubmitOrder())
        {
            var orderId = Guid.NewGuid();
            var actor = GetOrderProcessingActor(orderId);
            var cartItems = JsonSerializer.Deserialize<List<CartItem>>(cloudEvent.Data?.ToString() ?? "");
            await actor.SubmitAsync(cartItems);
            
            _logger.LogInformation("Order accepted. OrderId: {OrderId}", orderId);
            return Ok();
        }
        else
        {
            _logger.LogInformation($"Order status: retry");
            return StatusCode((int)HttpStatusCode.InternalServerError);
        }
    }

    [HttpPost("/order_submitted")]
    public async Task<ActionResult> OrderSubmitted()
    {
        var jsonFormatter = new JsonEventFormatter();
        var cloudEvent = await Request.ToCloudEventAsync(jsonFormatter);
        
        _logger.LogInformation("Order submitted - {Data}", cloudEvent.Data);
        return Ok();
    }

    [HttpPost("/order_paid")]
    public async Task<ActionResult> OrderPaid()
    {
        var jsonFormatter = new JsonEventFormatter();
        var cloudEvent = await Request.ToCloudEventAsync(jsonFormatter);
        
        var paidEvent = JsonSerializer.Deserialize<OrderPaidEvent>(cloudEvent.Data?.ToString() ?? "");
        if (paidEvent != null)
        {
            var actor = GetOrderProcessingActor(paidEvent.OrderId);
            await actor.ShipAsync();
        }

        return Ok();
    }

    [HttpPost("/order_payment_failed")]
    public async Task<ActionResult> OrderPaymentFailed()
    {
        var jsonFormatter = new JsonEventFormatter();
        var cloudEvent = await Request.ToCloudEventAsync(jsonFormatter);
        
        var paidEvent = JsonSerializer.Deserialize<OrderPaymentFailedEvent>(cloudEvent.Data?.ToString() ?? "");
        if (paidEvent != null)
        {
            var actor = GetOrderProcessingActor(paidEvent.OrderId);
            await actor.CancelAsync();
        }

        return Ok();
    }
    
    [HttpPost("/order_cancelled")]
    public async Task<ActionResult> OrderCancelled()
    {
        var jsonFormatter = new JsonEventFormatter();
        var cloudEvent = await Request.ToCloudEventAsync(jsonFormatter);
        
        _logger.LogInformation("Order cancelled - {Data}", cloudEvent.Data);
        return Ok();
    }
    
    [HttpPost("/order_shipped")]
    public async Task<ActionResult> OrderShipped()
    {
        var jsonFormatter = new JsonEventFormatter();
        var cloudEvent = await Request.ToCloudEventAsync(jsonFormatter);
        
        _logger.LogInformation("Order shipped - {Data}", cloudEvent.Data);
        return Ok();
    }

    
    [HttpPost("/simulate_pay")]
    public async Task<ActionResult> SimulatePay([FromBody] Guid orderId)
    {
        _logger.LogInformation("Payment received - {orderId}", orderId);
        
        var actor = GetOrderProcessingActor(orderId);
        await actor.NotifyPaymentSucceededAsync();

        return Ok();
    }

    #endregion
    

    #region Private Methods

    private bool TrySubmitOrder()
    {
        var result = _random.Next(1, 5);

        return result > 2;
    }
    
    private IOrderProcessingActor GetOrderProcessingActor(Guid orderId)
    {
        var actorId = new ActorId(orderId.ToString());
        
        return _actorProxyFactory.CreateActorProxy<IOrderProcessingActor>(
            actorId,
            nameof(OrderProcessingActor));
    }
    
    #endregion
}