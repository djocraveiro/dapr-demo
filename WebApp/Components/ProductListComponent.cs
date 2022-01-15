using WebApp.Events;
using WebApp.Models;
using WebApp.Services.Contracts;
using EventAggregator.Blazor;
using Microsoft.AspNetCore.Components;

namespace WebApp.Components;

public class ProductListComponent : ComponentBase
{
    [Inject]
    protected IProductService ProductService { get; set; }

    [Inject]
    protected ICartService CartService { get; set; }

    [Inject]
    private IEventAggregator EventAggregator { get; set; }

    protected IEnumerable<Product> products = null;
    protected Product selectedProduct;
    protected string selectedProductId;

    protected int curPage;
    protected bool hasPreviousPage;
    protected bool hasNextPage;
    private const int PageSize = 12;
    
    protected double minPrice = 0;
    protected double maxPrice = 1000;

    protected override async Task OnInitializedAsync()
    {
        if (products == null)
        {
            curPage = 1;
            await LoadPage();
        }
    }

    protected void SelectProduct(string productId)
    {
        selectedProductId = productId;
        selectedProduct = products.First(x => x.Id == productId);
    }

    protected async Task PreviousPage()
    {
        if (hasPreviousPage)
        {
            curPage--;
        }

        await LoadPage();
    }

    protected async Task NextPage()
    {
        if (hasNextPage)
        {
            curPage++;
        }

        await LoadPage();
    }

    private async Task LoadPage()
    {
        products = await ProductService.GetProducts(curPage, PageSize, minPrice, maxPrice);
        
        hasPreviousPage = curPage > 1;
        hasNextPage = products.Count() == PageSize;
    }

    protected async Task AddToCart(Product product)
    {
        var itemCount = await CartService.AddToCart(product);

        await EventAggregator.PublishAsync(new ShoppingCartUpdated 
        { 
            ItemCount = itemCount
        });
    }

    protected async Task Refresh()
    {
        await LoadPage();
    }
}