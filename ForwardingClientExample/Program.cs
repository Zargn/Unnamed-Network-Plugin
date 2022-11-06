using ForwardingClient;
using ForwardingClientExample.Commands;
using ForwardingClientExample.CommandSystem;
using ForwardingServer.Resources.CommandPackages;
using ForwardingServerExampleShared;
using Unnamed_Networking_Plugin;

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

        Program program = new(userName);
        program.Run();

        // Todo: use the FwClient to connect to a forwarding server and subscribe to all events.

        // Create a / based command system where /COMMAND is used to trigger special events.

        // Any regular non-command message should be forwarded to all clients in the current group.
    }



    public ConnectionState ConnectionState;

    private ITextCommand[] disconnectedCommands;
    private ITextCommand[] menuCommands;
    private ITextCommand[] groupCommands;

    private CommandFilter commandFilter;
    private FwClient fwClient;


    public Program(string username)
    {
        fwClient = new FwClient(new LogFileController(), new JsonSerializerAdapter(), new UserIdentificationPackage(new UserIdentification(username)));

        disconnectedCommands = new ITextCommand[]
        {
            new ConnectCommand(fwClient)
        };
        
        menuCommands = new ITextCommand[]
        {
            new ListGroupsCommand(),
        };

        groupCommands = new ITextCommand[]
        {

        };
        
        commandFilter = new CommandFilter(disconnectedCommands);

        SubscribeToPackages();
    }
    
    public void Run()
    {
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

    private void SubscribeToPackages()
    {
        fwClient.PackageBroker.SubscribeToPackage<InMenuPackage>(HandleInMenuPackage);
    }

    private async void HandleInMenuPackage(object? o, PackageReceivedEventArgs args)
    {
        ConnectionState = ConnectionState.ConnectedInMenu;
        commandFilter.SetCommandList(menuCommands);
    }
}

public enum ConnectionState
{
    Disconnected,
    ConnectedInMenu,
    ConnectedInGroup
}