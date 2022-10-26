using Unnamed_Networking_Plugin;

namespace Network_Plugin_User_frontend;

[Serializable]
public class TextMessagePackage : Package
{
    public string Message { get; init; }
    
    public TextMessagePackage(string message)
    {
        Message = message;
    }
}