using System.Text;
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

    public async Task<IEnumerable<Product>> GetProducts(int page, int limit, double? minPrice = null,
        double? maxPrice = null)
    {
        var query = BuildQuery(page, limit, minPrice, maxPrice);
        var response = await DaprClient.GetAsync(
            $"/v1.0/invoke/{_productsAppId}/method/api/products?{query}");

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

    private string BuildQuery(int page, int limit, double? minPrice = null,
        double? maxPrice = null)
    {
        var builder = new StringBuilder($"page={page}&limit={limit}");

        if (minPrice.HasValue)
        {
            builder.Append($"&minPrice={minPrice}");
        }
        
        if (maxPrice.HasValue)
        {
            builder.Append($"&maxPrice={maxPrice}");
        }

        return builder.ToString();
    }
}