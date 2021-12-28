using System.Text.Json;
using CloudNative.CloudEvents;

namespace OrderService.Extensions;

public static class CloudEventsExtensions
{
    public static T GetData<T>(this CloudEvent cloudEvent) where T : class
    {
        if (cloudEvent?.Data == null)
        {
            return null;
        }
        
        return ((JsonElement) cloudEvent.Data).Deserialize<T>();
    }
}