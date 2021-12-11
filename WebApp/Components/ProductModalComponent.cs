﻿using WebApp.Models;
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