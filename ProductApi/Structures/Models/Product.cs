using System.Text.Json.Serialization;

namespace ProductApi.Structures.Models;

public class Product
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("image")]
    public string Image { get; set; } = "";

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("description")]
    public string Description { get; set; } = "";

    //[JsonPropertyName("price")]
    public double Price { get; set; }
}