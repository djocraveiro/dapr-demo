using Microsoft.AspNetCore.Mvc;
using ProductApi.Services.Contracts;
using ProductApi.Structures.Models;
using ProductApi.Structures.Requests;

namespace ProductApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    
    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }
    
    [HttpGet]
    public async Task<ActionResult> GetList([FromQuery] ProductsRequest request)
    {
        if (request == null)
        {
            return BadRequest("invalid request");
        }

        var result = await _productService.GetAllProducts(request.Page, request.Limit,
            request.MinPrice, request.MaxPrice);

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetProductById(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            NotFound($"product {nameof(id)}");
        }

        var result = await _productService.GetProductById(id);

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        if (request == null)
        {
            return BadRequest("invalid request");
        }

        var product = new Product
        {
            Image = request.Image,
            Name = request.Name,
            Description = request.Description,
            Price = request.Price
        };

        var result = await _productService.CreateProduct(product);

        return Created(result.Id, result);
    }
}