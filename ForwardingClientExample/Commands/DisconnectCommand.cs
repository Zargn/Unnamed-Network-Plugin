using ForwardingClient;
using ForwardingClientExample.CommandSystem;

namespace ForwardingClientExample.Commands;

public class DisconnectCommand : ITextCommand
{
    public string CommandName => "disconnect";
    public string Syntax => "/disconnect";
    public string? Execute(string commandString)
    {
        client.PoolTask(client.DisconnectAsync());
        return null;
    }
    
    private FwClient client;

    public DisconnectCommand(FwClient fwClient)
    {
        client = fwClient;
    }
}