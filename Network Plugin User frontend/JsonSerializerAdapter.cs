using System.Text.Json;
using Unnamed_Networking_Plugin.Interfaces;

namespace Network_Plugin_User_frontend;

public class JsonSerializerAdapter : IJsonSerializer
{
    private readonly JsonSerializerOptions options = new() {IncludeFields = true};

    public string? Serialize<T>(T serializableClass)
    {
        return JsonSerializer.Serialize(serializableClass, options);
    }

    public T? DeSerialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, options);
    }

    public object? Deserialize(string json, Type type)
    {
        return JsonSerializer.Deserialize(json, type, options);
    }
}