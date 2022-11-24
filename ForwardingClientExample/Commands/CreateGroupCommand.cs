using ForwardingClient;
using ForwardingClientExample.CommandSystem;

namespace ForwardingClientExample.Commands;

public class CreateGroupCommand : ITextCommand
{
    public string CommandName => "creategroup";
    public string Syntax => "/creategroup *title(Max 32 characters)* *description* *max users*";
    public string? Execute(string commandString)
    {
        var arguments = CommandExtractor.GetArguments(commandString);
        
        if (arguments.Count != 3)
        {
            return "Invalid arguments. Syntax: " + Syntax;
        }

        if (arguments[0].Length > 32)
        {
            return "Title was too long. Syntax: " + Syntax;
        }

        int maxUsers;
        
        try
        {
            maxUsers = Int32.Parse(arguments[2]);
        }
        catch (FormatException)
        {
            return "Max users was not a number. Syntax: " + Syntax;
        }
        
        client.PoolTask(client.SendCreateGroupRequest(maxUsers, arguments[0], arguments[1]));
        return null;
    }
    
    private FwClient client;

    public CreateGroupCommand(FwClient fwClient)
    {
        client = fwClient;
    }
}