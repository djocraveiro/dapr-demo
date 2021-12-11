using WebApp.Services.Contracts;
using WebApp.Models;
using System.Net;
using System.Text;
using System.Text.Json;

namespace WebApp.Services;

public class DaprCartService : ICartService
{
    private readonly IHttpClientFactory _clientFactory;
    private HttpClient DaprClient => _clientFactory.CreateClient("dapr");
    private const string CartStoreKey = "cart";
    private readonly string _cartStore;
    private readonly string _cartEventBus;

    public DaprCartService(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
        _cartStore = Environment.GetEnvironmentVariable("CART_STORE");
        _cartEventBus = Environment.GetEnvironmentVariable("CART_EVENT_BUS");
    }

    public async Task<int> AddToCart(Product product)
    {
        var cartState = await ReadCart();

        bool isAlreadyInCart = cartState.ContainsKey(product.Id);

        if (isAlreadyInCart)
        {
            var selectedItem = cartState[product.Id];
            selectedItem.Quantity++;
            cartState[product.Id] = selectedItem;
        }
        else 
        {
            cartState[product.Id] = new CartItem 
            {
                Id = product.Id,
                Name = product.Name,
                Quantity = 1,
                Price = product.Price
            };
        }

        await SaveCart(cartState);

        return cartState.Keys.Count;
    }

    public async Task<int> RemoveFromCart(string productId)
    {
        var cartState = await ReadCart();
        if (cartState.ContainsKey(productId))
        {
            cartState.Remove(productId);
        }

        await SaveCart(cartState);

        return cartState.Keys.Count;
    }

    public async Task<IEnumerable<CartItem>> GetCartItems()
    {
        var cartState =  await ReadCart();
        return cartState.Values;
    }

    public async Task SubmitCart(IEnumerable<CartItem> items)
    {
        var payload = JsonSerializer.Serialize(items);
        var content = new StringContent(payload, Encoding.UTF8, "application/json");

        await DaprClient.PostAsync($"v1.0/publish/{_cartEventBus}", content);

        await ClearCart();
    }

    public Task ClearCartItems()
    {
        return ClearCart();
    }

    private async Task SaveCart(Dictionary<string, CartItem> cartState)
    {
        var payload = JsonSerializer.Serialize(new[] 
        {
            new { key = CartStoreKey, value = cartState }
        });

        var content = new StringContent(payload, Encoding.UTF8, "application/json");
        await DaprClient.PostAsync($"v1.0/state/{_cartStore}", content);
    }

    private async Task<Dictionary<string, CartItem>> ReadCart()
    {
        var response = await DaprClient.GetAsync($"v1.0/state/{_cartStore}/{CartStoreKey}");
        if (!response.IsSuccessStatusCode || response.StatusCode != HttpStatusCode.OK)
        {
            return new Dictionary<string, CartItem>();
        }

        var responseBody = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Dictionary<string, CartItem>>(responseBody);

    }

    private Task ClearCart()
    {
        return DaprClient.DeleteAsync($"v1.0/state/{_cartStore}/{CartStoreKey}");
    }
}