using WebApp.Services;
using WebApp.Services.Contracts;
using EventAggregator.Blazor;

namespace WebApp;

public class Startup
{
    public IConfiguration Configuration { get; }
    
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddRazorPages();
        services.AddServerSideBlazor();

        services.AddHttpClient("dapr", c =>
        {
            c.BaseAddress = new Uri("http://localhost:3500");
            c.DefaultRequestHeaders.Add("User-Agent", typeof(Program).Assembly.GetName().Name);
        });
        
        services.AddControllers();
        services.AddScoped<IEventAggregator, EventAggregator.Blazor.EventAggregator>();
        services.AddSingleton<IProductService, DaprProductService>();
        services.AddSingleton<ICartService, DaprCartService>();
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseExceptionHandler("/Error");

        app.UseHsts();
        app.UseStaticFiles();

        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapRazorPages();
            endpoints.MapControllers();
            endpoints.MapBlazorHub();
        });
    }
}