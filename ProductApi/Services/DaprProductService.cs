using System.Net;
using ProductApi.Services.Contracts;
using System.Text.Json;
using ProductApi.Structures.Models;

namespace ProductApi.Services;

public class DaprProductService : IProductService
{
    private readonly IHttpClientFactory _clientFactory;
    private HttpClient DaprClient => _clientFactory.CreateClient("dapr");
    string DAPR_STORE_NAME = "mongodb-products-store";
    private readonly ILogger<DaprProductService> _logger;


    public DaprProductService(IHttpClientFactory clientFactory, ILogger<DaprProductService> logger)
    {
        _clientFactory = clientFactory;
        _logger = logger;
    }


    public async Task<IEnumerable<Product>> GetAllProducts(int page = 1, int limit = 20)
    {
        int skip = (page - 1) * limit;
        var content = new
        {
            query = new
            {
                sort = new []
                {
                    new {
                        key = "value.name",
                        order = "ASC"
                    }
                },
                pagination = new
                {
                    limit = limit,
                    token = skip.ToString()
                }
            }
        };

        var response = await DaprClient.PostAsJsonAsync($"v1.0-alpha1/state/{DAPR_STORE_NAME}/query", content);
        if (!response.IsSuccessStatusCode || response.StatusCode != HttpStatusCode.OK)
        {
            _logger.LogError(await response.Content.ReadAsStringAsync());
            return null;
        }

        var responseBody = await response.Content.ReadAsStringAsync();
        var queryResponse = JsonSerializer.Deserialize<QueryResponse<Product>>(responseBody);
        if (queryResponse?.Results == null)
        {
            return new List<Product>(0);
        }

        return queryResponse.Results
            .Select(x =>
            {
                x.Data.Id = x.Key;
                return x.Data;
            })
            .ToList();
    }

    public async Task<Product> GetProductById(string productId)
    {
        var response = await DaprClient.GetAsync($"v1.0/state/{DAPR_STORE_NAME}/{productId}");
        if (!response.IsSuccessStatusCode || response.StatusCode != HttpStatusCode.OK)
        {
            return null;
        }

        var responseBody = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Product>(responseBody);
    }

    public Task<Product> CreateProduct(Product product)
    {
        throw new NotImplementedException();
    }
}