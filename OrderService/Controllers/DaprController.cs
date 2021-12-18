using CloudNative.CloudEvents.AspNetCore;
using CloudNative.CloudEvents.SystemTextJson;
using Microsoft.AspNetCore.Mvc;

namespace OrderService.Controllers;

[ApiController]
[Route("[controller]")]
public class DaprController : ControllerBase
{
    private readonly ILogger<DaprController> _logger;
    
    public DaprController(ILogger<DaprController> logger)
    {
        _logger = logger;
    }

    [HttpGet("subscribe")]
    public ActionResult Subscribe()
    {
        _logger.LogInformation("Subscription made.");
        var payload = new[]
        {
            new
            {
                pubsubname = "rabbitmqbus",
                topic= "cart_checkout",
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

        return Ok();
    }
}