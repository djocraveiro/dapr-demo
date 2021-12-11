using System.Text.Json;
using WebApp.Models;
using WebApp.Services.Contracts;

namespace WebApp.Services;

public class DaprProductService : IProductService
{
    private readonly IHttpClientFactory _clientFactory;
    private HttpClient DaprClient => _clientFactory.CreateClient("dapr");
    private readonly string _productsAppId;

    public DaprProductService(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
        _productsAppId = Environment.GetEnvironmentVariable("PRODUCTS_APP_ID");
    }

    public async Task<IEnumerable<Product>> GetProducts(int page, int limit)
    {
        var response = await DaprClient.GetAsync($"/v1.0/invoke/{_productsAppId}/method/api/products?page={page}&limit={limit}");

        if (!response.IsSuccessStatusCode)
        {
            // probably log some stuff here
            return Enumerable.Empty<Product>();
        }
        var contentStream = await response.Content.ReadAsStreamAsync();
        var products = await JsonSerializer.DeserializeAsync<IEnumerable<Product>>(contentStream,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return products;
    }
}