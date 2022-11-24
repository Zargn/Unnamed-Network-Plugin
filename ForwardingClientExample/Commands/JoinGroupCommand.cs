using ForwardingClient;
using ForwardingClientExample.CommandSystem;

namespace ForwardingClientExample.Commands;

public class JoinGroupCommand : ITextCommand
{
    public string CommandName => "joingroup";
    public string Syntax => "/joingroup *group title*";
    public string? Execute(string commandString)
    {
        var arguments = CommandExtractor.GetArguments(commandString);

        if (arguments.Count != 1)
            return "Invalid arguments. Syntax: " + Syntax;
        
        if (arguments[0].Length > 32)
        {
            return "Title was too long. Syntax: " + Syntax;
        }

        client.PoolTask(client.SendJoinGroupRequest(0, arguments[0], ""));
        return null;
    }
    
    private FwClient client;

    public JoinGroupCommand(FwClient fwClient)
    {
        client = fwClient;
    }
}