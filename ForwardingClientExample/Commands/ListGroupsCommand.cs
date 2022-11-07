using ForwardingClient;
using ForwardingClientExample.CommandSystem;

namespace ForwardingClientExample.Commands;



public class ListGroupsCommand : ITextCommand
{
    public ListGroupsCommand(FwClient fwClient)
    {
        this.fwClient = fwClient;
    }

    public string CommandName => "list";
    public string Syntax => "/list";

    public string? Execute(string commandString)
    {
        fwClient.PoolTask(fwClient.SendListGroupsRequest());

        return "GroupList: null";
    }

    private FwClient fwClient;
}