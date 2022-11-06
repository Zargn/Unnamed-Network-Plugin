﻿using System.Text.RegularExpressions;
using ForwardingClient;
using ForwardingClientExample.Commands;
using ForwardingClientExample.CommandSystem;
using ForwardingServer.Resources;
using ForwardingServer.Resources.CommandPackages;
using ForwardingServer.Resources.InformationPackages;
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
        fwClient.ClientDisconnected += HandleClientDisconnected;
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
        fwClient.PackageBroker.SubscribeToPackage<GroupsListPackage>(HandleGroupsListPackage);
        fwClient.PackageBroker.SubscribeToPackage<InGroupPackage>(HandleInGroupPackage);
        fwClient.PackageBroker.SubscribeToPackage<ClientJoinedGroupPackage>(HandleClientJoinedGroupPackage);
        fwClient.PackageBroker.SubscribeToPackage<ClientLeftGroupPackage>(HandleClientLeftGroupPackage);
        fwClient.PackageBroker.SubscribeToPackage<GroupInformationPackage>(HandleGroupInformationPackage);
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
        var package = args.ReceivedPackage as ClientJoinedGroupPackage;
        Console.WriteLine($"{package.ClientInformation} Joined the group.");
    }
    
    private void HandleClientLeftGroupPackage(object? o, PackageReceivedEventArgs args)
    {
        var package = args.ReceivedPackage as ClientLeftGroupPackage;
        Console.WriteLine($"{package.ConnectionInformation} Left the group.");
    }
    
    private void HandleGroupInformationPackage(object? o, PackageReceivedEventArgs args)
    {
        var package = args.ReceivedPackage as GroupInformationPackage;
        Console.WriteLine("Current group:");
        Console.WriteLine(package.GroupInformation);
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
}

public enum ConnectionState
{
    Disconnected,
    ConnectedInMenu,
    ConnectedInGroup
}