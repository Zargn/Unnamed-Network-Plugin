using System.ComponentModel.Design;
using ForwardingClientExample.Commands;
using ForwardingClientExample.CommandSystem;

namespace ForwardingClientExample;

public class Program
{
    private static ITextCommand[] MenuCommands =
    {
        new ListGroupsCommand()
    };
    
    public static void Main()
    {
        var commandFilter = new CommandFilter(MenuCommands);
        
        while (true)
        {
            var input = Console.ReadLine();
            if (input[0] == '/')
            {
                Console.WriteLine(commandFilter.TryFindAndExecuteCommand(input.Substring(1)));
            }
            
            
        }
        // Todo: use the FwClient to connect to a forwarding server and subscribe to all events.
        
        // Create a / based command system where /COMMAND is used to trigger special events.
        
        // Any regular non-command message should be forwarded to all clients in the current group.
    }
}