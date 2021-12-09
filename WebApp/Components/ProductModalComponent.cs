using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Events;
using WebApp.Models;
using WebApp.Services.Contracts;
using EventAggregator.Blazor;
using Microsoft.AspNetCore.Components;

namespace WebApp.Components;

public class ProductModalComponent : ComponentBase
{
    [Parameter]
    public Product Product { get; set; }

    protected override void OnParametersSet()
    {
        if (Product == null)
        {
            Product = new Product();
        }

        base.OnParametersSet();
    }
}