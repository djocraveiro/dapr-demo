using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Events;
using WebApp.Models;
using WebApp.Services.Contracts;
using EventAggregator.Blazor;
using Microsoft.AspNetCore.Components;

namespace WebApp.Components;

public class CheckoutModalComponent : ComponentBase, EventAggregator.Blazor.IHandle<CheckoutStarted>
{
    [Inject]
    private IEventAggregator EventAggregator { get; set; }

    [Inject]
    private ICartService CartService { get; set; }

    protected IEnumerable<CartItem> Items { get; set; }

    protected double totalPrice = 0;

    protected override void OnInitialized()
    {
        EventAggregator.Subscribe(this);
    }
    
    public async Task HandleAsync(CheckoutStarted cartUpdated)
    {
        Items = await CartService.GetCartItems();
        totalPrice = GetTotalPrice();

        StateHasChanged();
    }

    protected async Task SubmitCheckout()
    {
        if (Items == null || !Items.Any())
        {
            return;
        }

        await CartService.SubmitCart(Items);

        Items = new CartItem[] {};
        totalPrice = 0;

        await EventAggregator.PublishAsync(new ShoppingCartUpdated
        {
            ItemCount = Items.Count()
        });

        StateHasChanged();
    }

    private double GetTotalPrice()
    {
        double total = 0;

        foreach(var item in Items)
        {
            total += (item.Price * item.Quantity);
        }

        return total;
    }
}