using ForwardingClient;
using ForwardingClientExample.CommandSystem;

namespace ForwardingClientExample.Commands;

public class ListUsersCommand : ITextCommand
{
    public string CommandName => "listusers";
    public string Syntax => "/listusers";
    public string? Execute(string commandString)
    {
        client.PoolTask(client.SendUserListRequest());
        return null;
    }
    
    private FwClient client;

    public ListUsersCommand(FwClient fwClient)
    {
        client = fwClient;
    }
}