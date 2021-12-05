using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebApp.Models;

namespace WebApp.Services;

public class DaprProductService : IProductService
{
    private readonly IHttpClientFactory clientFactory;

    public DaprProductService(IHttpClientFactory clientFactory)
    {
        this.clientFactory = clientFactory;
    }

    public async Task<IEnumerable<Product>> GetProducts(int page, int limit)
    {
        var client = clientFactory.CreateClient("dapr");
        var resp = await client.GetAsync($"/v1.0/invoke/productsapi/method/api/products?page={page}&limit={limit}");

        if (!resp.IsSuccessStatusCode)
        {
            // probably log some stuff here
            return Enumerable.Empty<Product>();
        }
        var contentStream = await resp.Content.ReadAsStreamAsync();
        var products = await JsonSerializer.DeserializeAsync<IEnumerable<Product>>(contentStream,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return products;
    }
}