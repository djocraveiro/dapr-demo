using System.Text.Json;
using System.Text.Json.Serialization;
using MongoDB.Driver;
using ProductApi.Services.Contracts;
using ProductApi.Services;


var builder = WebApplication.CreateBuilder(args);


#region Add services to the container

builder.Services.AddControllers()
    .AddJsonOptions(option =>
    {
        option.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        option.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

builder.Services.AddSingleton<IMongoClient>(provider =>
    {
        const string secretKey = "mongodbConnString";
        using var client = new Dapr.Client.DaprClientBuilder().Build();
        var secret = client.GetSecretAsync("localsecrets", secretKey).Result;
        
        var mongodbConnString = secret?[secretKey];
        if (string.IsNullOrWhiteSpace(mongodbConnString))
        {
            throw new Exception($"invalid {secretKey}");
        }

        return new MongoClient(mongodbConnString);
    });

builder.Services.AddScoped<IProductService, ProductService>();

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