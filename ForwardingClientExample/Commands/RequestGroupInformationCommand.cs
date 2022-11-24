using ForwardingClient;
using ForwardingClientExample.CommandSystem;

namespace ForwardingClientExample.Commands;

public class RequestGroupInformationCommand : ITextCommand
{
    public string CommandName => "groupinfo";
    public string Syntax => "/groupinfo";
    public string? Execute(string commandString)
    {
        client.PoolTask(client.SendGroupInformationRequest());
        return null;
    }
    
    private FwClient client;

    public RequestGroupInformationCommand(FwClient fwClient)
    {
        client = fwClient;
    }
}