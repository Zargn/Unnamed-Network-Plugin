using ForwardingServerExampleShared;
using Unnamed_Networking_Plugin.Resources;

namespace ForwardingClientExample;

public class PrivateMessagePackage : Package
{
    public string Message { get; init; }
    public UserIdentification Sender { get; init; }
    
    public PrivateMessagePackage(string message, UserIdentification sender)
    {
        Message = message;
        Sender = sender;
    }
}