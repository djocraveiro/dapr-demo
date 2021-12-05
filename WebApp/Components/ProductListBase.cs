using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebApp.Events;
using WebApp.Models;
using WebApp.Services;
using EventAggregator.Blazor;
using Microsoft.AspNetCore.Components;

namespace WebApp.Components;

public class ProductListBase : ComponentBase
{
    [Inject]
    protected IProductService ProductService { get; set; }

    [Inject]
    private IEventAggregator EventAggregator { get; set; }

    [Inject]
    private IHttpClientFactory ClientFactory { get; set; }

    protected IEnumerable<Product> products = null;
    protected Product selectedProduct;
    protected string selectedProductId;

    protected int curPage;
    protected bool hasPreviousPage;
    protected bool hasNextPage;
    private const int PageSize = 12;

    protected override async Task OnInitializedAsync()
    {
        if (products == null)
        {
            curPage = 1;
            await LoadPage();
        }
    }

    protected void SelectProduct(string productId)
    {
        selectedProductId = productId;
        //selectedProduct = (await ProductService.GetProducts()).First(x => x.Id == productId);
        selectedProduct = products.First(x => x.Id == productId);
    }

    protected async Task PreviousPage()
    {
        if (hasPreviousPage)
        {
            curPage--;
        }

        await LoadPage();
    }

    protected async Task NextPage()
    {
        if (hasNextPage)
        {
            curPage++;
        }

        await LoadPage();
    }

    private async Task LoadPage()
    {
        products = await ProductService.GetProducts(curPage, PageSize);
        hasPreviousPage = curPage > 1;
        hasNextPage = products.Count() == PageSize;
    }

    protected async Task AddToCart(string productId, string name)
    {
        //TODO
        // get state
        /*var client = ClientFactory.CreateClient("dapr");
        var resp = await client.GetAsync($"v1.0/state/{Constants.STORE_NAME}/cart");

        if (!resp.IsSuccessStatusCode) return;

        Dictionary<string, CartItem> state = null;
        if (resp.StatusCode == HttpStatusCode.NoContent)
        {
            // Empty cart
            state = new Dictionary<string, CartItem> { [productId] = new CartItem { Name = name, Quantity = 1 } };
        }
        else if (resp.StatusCode == HttpStatusCode.OK)
        {
            var responseBody = await resp.Content.ReadAsStringAsync();
            state = JsonSerializer.Deserialize<Dictionary<string, CartItem>>(responseBody);
            if (state.ContainsKey(productId))
            {
                // Product already in cart
                CartItem selectedItem = state[productId];
                selectedItem.Quantity++;
                state[productId] = selectedItem;
            }
            else
            {
                // Add product to car
                state[productId] = new CartItem { Name = name, Quantity = 1 };
            }
        }

        // persist state in dapr
        var payload = JsonSerializer.Serialize(new[] {
            new { key = "cart", value = state }
        });

        var content = new StringContent(payload, Encoding.UTF8, "application/json");
        await client.PostAsync($"v1.0/state/{Constants.STORE_NAME}", content);
        await EventAggregator.PublishAsync(new ShoppingCartUpdated { ItemCount = state.Keys.Count });*/
    }
}