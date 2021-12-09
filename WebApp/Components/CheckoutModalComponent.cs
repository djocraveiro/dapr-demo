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
    private IHttpClientFactory ClientFactory { get; set; }

    protected IEnumerable<CartItem> Items { get; set; }

    protected override void OnInitialized()
    {
        EventAggregator.Subscribe(this);
    }
    
    public async Task HandleAsync(CheckoutStarted cartUpdated)
    {
        //TODO
        /*// get state
        var client = ClientFactory.CreateClient("dapr");
        var resp = await client.GetAsync($"v1.0/state/{Constants.STORE_NAME}/cart");

        if (!resp.IsSuccessStatusCode || resp.StatusCode == HttpStatusCode.NoContent) return;

        var responseBody = await resp.Content.ReadAsStringAsync();
        var state = JsonSerializer.Deserialize<Dictionary<string, CartItem>>(responseBody);
        Items = state.Values;*/
        StateHasChanged();
    }

    protected async Task SubmitCheckout()
    {
        //TODO
        /*if (Items == null || !Items.Any()) return;

        var client = ClientFactory.CreateClient("dapr");
        var resp = await client.DeleteAsync($"v1.0/state/{Constants.STORE_NAME}/cart");

        var payload = JsonSerializer.Serialize(Items);
        var content = new StringContent(payload, Encoding.UTF8, "application/json");
        await client.PostAsync($"v1.0/publish/{Constants.PUBSUB_NAME}/checkout", content);

        Items = null;
        await EventAggregator.PublishAsync(new ShoppingCartUpdated { ItemCount = 0 });*/
        StateHasChanged();
    }
}