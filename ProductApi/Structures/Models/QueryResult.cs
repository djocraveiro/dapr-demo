using System.Text.Json.Serialization;

namespace ProductApi.Structures.Models;

internal class QueryResult<T>
{
    [JsonPropertyName("key")]
    public string Key { get; set; }
        
    [JsonPropertyName("data")]
    public T Data { get; set; }
        
    [JsonPropertyName("etag")]
    public string Etag { get; set; }
}