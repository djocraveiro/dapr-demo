using System.ComponentModel.DataAnnotations;

namespace ProductApi.Structures.Requests;

public class CreateProductRequest
{
    public string Image { get; set; } = "";

    [Required]
    [StringLengthAttribute(64)]
    public string Name { get; set; } = "";

    public string Description { get; set; } = "";

    [Required]
    [Range(0, float.MaxValue)]
    public float Price { get; set; }
}