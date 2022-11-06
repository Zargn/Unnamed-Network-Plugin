using ForwardingClient;
using ForwardingClientExample.Commands;
using ForwardingClientExample.CommandSystem;
using ForwardingServerExampleShared;

namespace ForwardingClientExample;

public class Program
{
    public static void Main()
    {
        Console.WriteLine("Please enter UserName: ");
        var userName = Console.ReadLine() ?? "NoName";

        if (userName == "")
        {
            userName = "NoName";
        }

        Program program = new();
        program.Run(userName);

        // Todo: use the FwClient to connect to a forwarding server and subscribe to all events.

        // Create a / based command system where /COMMAND is used to trigger special events.

        // Any regular non-command message should be forwarded to all clients in the current group.
    }

    public enum ConnectionState
    {
        Disconnected,
        ConnectedInMenu,
        ConnectedInGroup
    }

    public void Run(string username)
    {
        var fwClient = new FwClient(new LogFileController(), new JsonSerializerAdapter(), new UserIdentificationPackage(new UserIdentification(username)));

        ITextCommand[] menuCommands =
        {
            new ListGroupsCommand(),
            new ConnectCommand(fwClient)
        };
        
        var commandFilter = new CommandFilter(menuCommands);
        
        while (true)
        {
            var input = Console.ReadLine();
            if (input[0] == '/')
            {
                Console.WriteLine(commandFilter.TryFindAndExecuteCommand(input.Substring(1)));
            }
            
            
        }
    }
}