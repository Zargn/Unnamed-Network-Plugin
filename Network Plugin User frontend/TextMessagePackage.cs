using Unnamed_Networking_Plugin;
using Unnamed_Networking_Plugin.Packages;

namespace Network_Plugin_User_frontend;

[Serializable]
public class TextMessagePackage : Package
{
    public string Message { get; init; }

    public NameIdentifier Identifier { get; init; }
    
    public TextMessagePackage(string message, NameIdentifier identifier)
    {
        Message = message;
        Identifier = identifier;
    }

    public override string ToString()
    {
        return $"({Identifier.Name}): {Message}";
    }
}