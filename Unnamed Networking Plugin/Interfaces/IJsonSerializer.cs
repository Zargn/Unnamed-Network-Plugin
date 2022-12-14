namespace Unnamed_Networking_Plugin.Interfaces;

public interface IJsonSerializer
{
    public string? Serialize<T>(T serializableClass);

    public string? Serialize(object? serializableClass, Type type);
    
    public T? DeSerialize<T>(string json);
    public object? Deserialize(string json, Type type);
}