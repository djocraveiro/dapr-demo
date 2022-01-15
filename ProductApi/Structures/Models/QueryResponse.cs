using System.Text.Json.Serialization;

namespace ProductApi.Structures.Models;

internal class QueryResponse<T>
{
    [JsonPropertyName("results")]
    public IList<QueryResult<T>> Results { get; set; }
}