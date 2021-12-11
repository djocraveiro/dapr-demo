using ProductApi.Structures.Documents;
using ProductApi.Structures.Models;

namespace ProductApi.Extensions;

public static class ProductExtensions
{
    public static Product ToProductModel(this ProductDocument document)
    {
        if (document == null)
        {
            return null;
        }

        return new Product
        {
            Id = document.Id.ToString(),
            Name = document.Name,
            Image = document.Image,
            Description = document.Description,
            Price = document.Price
        };
    }

    public static ProductDocument ToProductDocument(this Product product)
    {
        if (product == null)
        {
            return null;
        }

        return new ProductDocument
        {
            Name = product.Name,
            Image = product.Image,
            Description = product.Description,
            Price = product.Price
        };
    }
}