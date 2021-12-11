using System.Linq;
using System.Threading.Tasks;
using WebApp.Events;
using WebApp.Services.Contracts;
using EventAggregator.Blazor;
using Microsoft.AspNetCore.Components;

namespace WebApp.Components;

public class ShoppingCartComponent : ComponentBase, IDisposable, EventAggregator.Blazor.IHandle<ShoppingCartUpdated>
{
    [Inject]
    private ICartService CartService { get; set; }

    [Inject]
    private IEventAggregator EventAggregator { get; set; }

    [Parameter]
    public string CheckoutModalTarget { get; set; }
    protected int shoppingCartCount = 0;

    protected override async Task OnInitializedAsync()
    {
        EventAggregator.Subscribe(this);

        shoppingCartCount = (await CartService.GetCartItems())?.Count() ?? 0;
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