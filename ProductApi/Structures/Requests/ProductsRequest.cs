using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ProductApi.Structures.Requests;

public class ProductsRequest
{
    [BindRequired]
    public int Page { get; set; }

    [BindRequired]
    public int Limit {get; set; }
}