using System.Net;
using System.Text.RegularExpressions;
using ForwardingClient;
using ForwardingClientExample.Commands;
using ForwardingClientExample.CommandSystem;
using ForwardingServer.Resources;
using ForwardingServer.Resources.CommandPackages;
using ForwardingServer.Resources.InformationPackages;
using ForwardingServerExampleShared;
using Unnamed_Networking_Plugin;
using Unnamed_Networking_Plugin.Interfaces;

namespace ForwardingClientExample;

public class Program
{
    public static async Task Main()
    {
        Console.WriteLine("Please enter UserName: ");
        var userName = Console.ReadLine() ?? "NoName";

        if (userName == "")
        {
            userName = "NoName";
        }


        Console.WriteLine("Write y to enable debug logs. Press enter to skip...");
        var input = Console.ReadLine();

        var enableDebugLogs = input.ToLower() == "y";
            
            
        Program program = new(userName, enableDebugLogs);
        var runTask = program.Run();

        await runTask;

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

    private UserIdentification userIdentification;
    private IJsonSerializer jsonSerializer;
    private LogFileController logFileController;
    
    
    public Program(string username, bool printLog)
    {
        userIdentification = new UserIdentification(username);
        jsonSerializer = new JsonSerializerAdapter();
        logFileController = new LogFileController(printLog);
        
        fwClient = new FwClient(logFileController, jsonSerializer, new UserIdentificationPackage(userIdentification));

        disconnectedCommands = new ITextCommand[]
        {
            new ConnectCommand(fwClient),
            new PrintLogsCommand(logFileController),
        };
        
        menuCommands = new ITextCommand[]
        {
            new ListGroupsCommand(fwClient),
            new CreateGroupCommand(fwClient),
            new JoinGroupCommand(fwClient),
            new DisconnectCommand(fwClient),
            new PrintLogsCommand(logFileController),
        };

        groupCommands = new ITextCommand[]
        {
            new DisconnectCommand(fwClient),
            new LeaveGroupCommand(fwClient),
            new RequestGroupInformationCommand(fwClient),
            new SendPrivateMessageCommand(fwClient, userIdentification),
            new ListUsersCommand(fwClient),
            new PrintLogsCommand(logFileController),
        };
        
        commandFilter = new CommandFilter(disconnectedCommands);

        SubscribeToPackages();
        fwClient.ClientDisconnected += HandleClientDisconnected;
        fwClient.ClientConnected += HandleClientConnected;
    }
    
    public async Task Run()
    {
        CancellationTokenSource cancelPool = new();
        var taskPool = fwClient.GetTaskPoolTask(cancelPool.Token);

        
        while (true)
        {
            if (taskPool.IsCompleted)
            {
                await taskPool;
            }
            
            var input = Console.ReadLine();

            if (input == null)
            {
                Console.WriteLine("Input was null. Ending loop.");
                break;
            }
            
            if (input == "")
                continue;

            if (input == "/quit")
                break;

            if (input[0] == '/')
            {
                var output = commandFilter.TryFindAndExecuteCommand(input[1..]);
                if (output != null)
                    Console.WriteLine(output);
            }
            
            else if (ConnectionState == ConnectionState.ConnectedInGroup)
            {
                var package = new MessagePackage(input, userIdentification);
                fwClient.PoolTask(fwClient.SendPackageToAllGroupMembers(package));
            }

            
        }

        await taskPool;
    }

    
    // /connect *127.0.0.1* *25564*
    // /creategroup *testgroup* *this is a test* *5*
    // 


    private void SubscribeToPackages()
    {
        fwClient.PackageBroker.SubscribeToPackage<InMenuPackage>(HandleInMenuPackage);
        fwClient.PackageBroker.SubscribeToPackage<GroupsListPackage>(HandleGroupsListPackage);
        
        fwClient.PackageBroker.SubscribeToPackage<InGroupPackage>(HandleInGroupPackage);
        fwClient.PackageBroker.SubscribeToPackage<ClientJoinedGroupPackage<UserIdentification>>(HandleClientJoinedGroupPackage);
        fwClient.PackageBroker.SubscribeToPackage<ClientLeftGroupPackage<UserIdentification>>(HandleClientLeftGroupPackage);
        fwClient.PackageBroker.SubscribeToPackage<GroupInformationPackage>(HandleGroupInformationPackage);
        fwClient.PackageBroker.SubscribeToPackage<PrivateMessagePackage>(HandlePrivateMessagePackage);
        fwClient.PackageBroker.SubscribeToPackage<MessagePackage>(HandleMessagePackage);
        
        fwClient.PackageBroker.SubscribeToPackage<WarningPackage>(HandleWarningPackage);
        fwClient.PackageBroker.SubscribeToPackage<ErrorPackage>(HandleErrorPackage);
    }

    
    // Menu
    private void HandleInMenuPackage(object? o, PackageReceivedEventArgs args)
    {
        ConnectionState = ConnectionState.ConnectedInMenu;
        commandFilter.SetCommandList(menuCommands);
    }

    private void HandleGroupsListPackage(object? o, PackageReceivedEventArgs args)
    {
        Console.WriteLine("Available groups: ");
        var package = args.ReceivedPackage as GroupsListPackage;
        foreach (var groupInformation in package.GroupInformation)
        {
            Console.WriteLine(groupInformation);
        }
    }
    
    // Group
    private void HandleInGroupPackage(object? o, PackageReceivedEventArgs args)
    {
        ConnectionState = ConnectionState.ConnectedInGroup;
        commandFilter.SetCommandList(groupCommands);
    }
    
    private void HandleClientJoinedGroupPackage(object? o, PackageReceivedEventArgs args)
    {
        var package = args.ReceivedPackage as ClientJoinedGroupPackage<UserIdentification>;
        Console.WriteLine($"{package.ClientInformation} Joined the group.");
    }
    
    private void HandleClientLeftGroupPackage(object? o, PackageReceivedEventArgs args)
    {
        var package = args.ReceivedPackage as ClientLeftGroupPackage<UserIdentification>;
        Console.WriteLine($"{package.ConnectionInformation} Left the group.");
    }
    
    private void HandleGroupInformationPackage(object? o, PackageReceivedEventArgs args)
    {
        var package = args.ReceivedPackage as GroupInformationPackage;
        Console.WriteLine("Current group:");
        Console.WriteLine(package.GroupInformation);
    }

    private void HandlePrivateMessagePackage(object? o, PackageReceivedEventArgs args)
    {
        var package = args.ReceivedPackage as PrivateMessagePackage;
        Console.WriteLine($"{package.Sender} to you: {package.Message}");
    }

    private void HandleMessagePackage(object? o, PackageReceivedEventArgs args)
    {
        var package = args.ReceivedPackage as MessagePackage;
        Console.WriteLine($"({package.Sender}): {package.Message}");
    }
    
    // General
    private void HandleWarningPackage(object? o, PackageReceivedEventArgs args)
    {
        var package = args.ReceivedPackage as WarningPackage;
        Console.WriteLine($"The remote server answered with the following warning:");
        Console.WriteLine($"[{package.WarningType.ToString()}]: {package.WarningMessage}");
    }
    
    private void HandleErrorPackage(object? o, PackageReceivedEventArgs args)
    {
        var package = args.ReceivedPackage as ErrorPackage;
        Console.WriteLine($"The remote server experienced the following error upon your request:");
        Console.WriteLine($"[{package.ErrorMessage}]:");
        Console.WriteLine(package.Exception);
    }

    private void HandleClientDisconnected(object? o, EventArgs args)
    {
        ConnectionState = ConnectionState.Disconnected;
        commandFilter.SetCommandList(disconnectedCommands);
    }

    private void HandleClientConnected(object? o, EventArgs args)
    {
        Console.WriteLine("Connection successful!");
    }
}



public enum ConnectionState
{
    Disconnected,
    ConnectedInMenu,
    ConnectedInGroup
}