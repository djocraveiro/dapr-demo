using System.Text.Json;
using System.Text.Json.Serialization;
using MongoDB.Driver;
using ProductApi.Services.Contracts;
using ProductsApi.Services;


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
        var config = provider.GetService<IConfiguration>();
        if (config == null)
        {
            throw new Exception("invalid configuration");
        }

        return new MongoClient(config["MONGO_CONNECTION"]);
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