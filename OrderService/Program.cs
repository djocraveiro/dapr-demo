using System.Text.Json;
using System.Text.Json.Serialization;
using OrderService.Actors;


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

builder.Services.AddActors(options =>
    {
        options.Actors.RegisterActor<OrderProcessingActor>();
    });

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
    endpoints.MapActorsHandlers();
});

app.Run();

#endregion