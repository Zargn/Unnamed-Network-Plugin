using ForwardingServerExampleShared;
using Unnamed_Networking_Plugin.Resources;

namespace ForwardingClientExample;

public class MessagePackage : Package
{
    public string Message { get; init; }
    public UserIdentification Sender { get; init; }
    
    public MessagePackage(string message, UserIdentification sender)
    {
        Message = message;
        Sender = sender;
    }
}