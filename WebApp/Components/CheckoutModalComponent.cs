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
        
        await ResetCartState();
        StateHasChanged();
    }

    protected async Task RemoveItem(string productId)
    {
        var itemCount = await CartService.RemoveFromCart(productId);
        
        await EventAggregator.PublishAsync(new ShoppingCartUpdated
        {
            ItemCount = itemCount
        });
        
        Items = Items.Where(item => item.Id != productId);
        StateHasChanged();
    }

    protected async Task ClearItems()
    {
        await CartService.ClearCartItems();
        
        await ResetCartState();
        StateHasChanged();
    }

    private Task ResetCartState()
    {
        Items = new CartItem[] {};
        totalPrice = 0;

        return EventAggregator.PublishAsync(new ShoppingCartUpdated
        {
            ItemCount = Items.Count()
        });
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