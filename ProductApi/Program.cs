using System.Text.Json;
using System.Text.Json.Serialization;
using MongoDB.Driver;
using ProductApi.Services.Contracts;
using ProductApi.Services;


var builder = WebApplication.CreateBuilder(args);


#region Add services to the container

builder.Services.AddHttpClient("dapr", c =>
    {
        c.BaseAddress = new Uri("http://localhost:3500");
        c.DefaultRequestHeaders.Add("User-Agent", typeof(Program).Assembly.GetName().Name);
    });

builder.Services.AddControllers()
    .AddJsonOptions(option =>
    {
        option.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        option.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

builder.Services.AddSingleton<IMongoClient>(provider =>
    {
        var clientFactory = provider.GetService<IHttpClientFactory>();
        if (clientFactory == null)
        {
            throw new Exception($"no {nameof(IHttpClientFactory)} provided");
        }

        const string secretKey = "mongodbConnString";
        const string secretStore = "localsecrets";
        using var daprClient = clientFactory?.CreateClient("dapr");
        var jsonDocument = daprClient
            .GetFromJsonAsync<JsonDocument>($"/v1.0/secrets/{secretStore}/{secretKey}")
            .GetAwaiter()
            .GetResult();

        var property = jsonDocument?.RootElement.GetProperty(secretKey);
        var mongodbConnString = property?.GetString();
        
        if (string.IsNullOrWhiteSpace(mongodbConnString))
        {
            throw new Exception($"invalid {secretKey}");
        }

        return new MongoClient(mongodbConnString);
    });

builder.Services.AddScoped<IProductService, ProductService>();
//builder.Services.AddScoped<IProductService, DaprProductService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#endregion

#region Configure the HTTP request pipeline

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();

#endregion