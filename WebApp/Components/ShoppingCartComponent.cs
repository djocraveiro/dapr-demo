using System.Linq;
using System.Threading.Tasks;
using WebApp.Events;
using EventAggregator.Blazor;
using Microsoft.AspNetCore.Components;

namespace WebApp.Components;

public class ShoppingCartComponent : ComponentBase, IDisposable, EventAggregator.Blazor.IHandle<ShoppingCartUpdated>
{
    [Inject]
    private IEventAggregator EventAggregator { get; set; }

    [Parameter]
    public string CheckoutModalTarget { get; set; }
    protected int shoppingCartCount = 0;

    protected override void OnInitialized()
    {
        EventAggregator.Subscribe(this);
    }

    protected async Task Checkout()
    {
        await EventAggregator.PublishAsync(new CheckoutStarted());
    }

    public Task HandleAsync(ShoppingCartUpdated cartUpdated)
    {
        shoppingCartCount = cartUpdated.ItemCount;
        InvokeAsync(() => StateHasChanged());
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        EventAggregator.Unsubscribe(this);
    }
}