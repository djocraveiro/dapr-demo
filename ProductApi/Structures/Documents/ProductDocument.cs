using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ProductApi.Structures.Documents;

public class ProductDocument
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("image")]
    public string Image { get; set; } = "";

    [BsonElement("name")]
    public string Name { get; set; } = "";

    [BsonElement("description")]
    public string Description { get; set; } = "";

    [BsonElement("price")]
    public double Price { get; set; }
}