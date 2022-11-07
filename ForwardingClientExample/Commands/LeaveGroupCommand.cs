using ForwardingClient;
using ForwardingClientExample.CommandSystem;

namespace ForwardingClientExample.Commands;

public class LeaveGroupCommand : ITextCommand
{
    public string CommandName => "leavegroup";
    public string Syntax => "/leavegroup";
    public string? Execute(string commandString)
    {
        client.PoolTask(client.SendLeaveGroupRequest());
        return null;
    }
    
    private FwClient client;

    public LeaveGroupCommand(FwClient fwClient)
    {
        client = fwClient;
    }
}