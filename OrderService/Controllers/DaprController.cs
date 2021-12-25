using System.Net;
using CloudNative.CloudEvents.AspNetCore;
using CloudNative.CloudEvents.SystemTextJson;
using Microsoft.AspNetCore.Mvc;

namespace OrderService.Controllers;

[ApiController]
[Route("[controller]")]
public class DaprController : ControllerBase
{
    private readonly Random _random = new ();
    private readonly ILogger<DaprController> _logger;
    private readonly (string pubsubname, string topic) _cartEventBus;
    
    public DaprController(ILogger<DaprController> logger)
    {
        _logger = logger;
        
        var cartEventBus = Environment.GetEnvironmentVariable("CART_EVENT_BUS");
        var parts = cartEventBus?.Split('/');
        if (parts is not { Length: 2 })
        {
            throw new ArgumentException("invalid CART_EVENT_BUS");
        }

        _cartEventBus = (parts.First(), parts.Last());
    }

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
            }
        };
        
        return Ok(payload);
    }

    [HttpPost("/cart_checkout")]
    public async Task<ActionResult> CheckoutOrder()
    {
        var jsonFormatter = new JsonEventFormatter();
        var cloudEvent = await Request.ToCloudEventAsync(jsonFormatter);
        
        _logger.LogInformation("Order received.");
        _logger.LogDebug($"Cloud event {cloudEvent.Id} {cloudEvent.Type} {cloudEvent.DataContentType}");
        
        if (TryAcceptOrder())
        {
            _logger.LogInformation($"Order status: accepted");
            return Ok();
        }
        else
        {
            _logger.LogInformation($"Order status: retry");
            return StatusCode((int)HttpStatusCode.InternalServerError);
        }
    }


    private bool TryAcceptOrder()
    {
        var result = _random.Next(1, 5);

        return result > 2;
    }
}