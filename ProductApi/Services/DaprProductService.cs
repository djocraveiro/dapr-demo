/*using ProductApi.Services.Contracts;
using Dapr.Client;
using ProductsApi.Structures.Models;
using System.Net.Http.Headers;

namespace ProductApi.Services;

public class DaprProductService : IProductService
{
    private readonly DaprClient _daprClient;
    string DAPR_STORE_NAME = "mongodb-products-store";


    public DaprProductService()
    {
        _daprClient = new DaprClientBuilder().Build();
    }


    public async Task<IEnumerable<Product>> GetAllProducts(int page = 1, int limit = 20)
    {
        throw new NotImplementedException();
        const string sortQuery = "{ \"key\": \"title\", \"order\": \"DESC\" }";
        string limitQuery = "{ \"pagination\": { \"limit\": " + limit + " } }";
        string query = "{ \"query\": { \"sort\": " + sortQuery + ", \"\": " + limitQuery + " } }";

        var request = new HttpRequestMessage
        {
            Content = new StringContent(query, System.Text.Encoding.UTF8)
        };

        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        
        var response = await _daprClient.InvokeMethodWithResponseAsync(request);

        return results;
    }

    public async Task<Product> GetProductById(string productId)
    {
        var product = await _daprClient.GetStateAsync<Product>(DAPR_STORE_NAME, productId.ToString());
        return product;
    }
}*/