﻿using ForwardingServer.Resources.CommandPackages;
using ForwardingServer.Resources.ForwardingPackages;
using ForwardingServer.Resources.InformationPackages;
using Unnamed_Networking_Plugin;
using Unnamed_Networking_Plugin.Broker;
using Unnamed_Networking_Plugin.EventArgs;
using Unnamed_Networking_Plugin.Interfaces;
using Unnamed_Networking_Plugin.Resources;

namespace ForwardingServer.Group;

public class ConnectionGroup
{
    public GroupInformation GroupInformation => new GroupInformation(
        groupSettings.MaxSize, groupSettings.Title,
        groupSettings.Description, MemberCount);
    public PackageBroker Broker { get; } = new();
    public int MemberCount => members.Count;
    
    private readonly GroupSettings groupSettings;
    private List<Connection> members = new();
    private IdentificationPackage package;
    private Type forwardingPackageType;
    private UnnamedNetworkPluginClient client;
    private FwServer fwServer;

    public ConnectionGroup(GroupSettings groupSettings, Type forwardingPackageType, UnnamedNetworkPluginClient client, FwServer fwServer)
    {
        this.groupSettings = groupSettings;
        this.forwardingPackageType = forwardingPackageType;
        this.client = client;
        this.fwServer = fwServer;

        SetUpSubscribers();
    }

    public bool Join(Connection connection)
    {
        if (MemberCount >= groupSettings.MaxSize)
            return false;
        
        connection.PackageReceived += Broker.InvokeSubscribers;
        connection.ClientDisconnected += HandleClientDisconnected;

        members.Add(connection);
        
        return true;
    }

    public void Leave(Connection connection)
    {
        connection.PackageReceived -= Broker.InvokeSubscribers;
        connection.ClientDisconnected -= HandleClientDisconnected;

        members.Remove(connection);
    }

    private void SetUpSubscribers()
    {
        Broker.SubscribeToPackage<LeaveGroupPackage>(HandleLeaveGroupPackage);
        Broker.SubscribeToPackage<ForwardingPackage>(HandleForwardingPackage);
        Broker.SubscribeToPackage<ForwardingPackageAll>(HandleForwardingPackageAll);
        Broker.SubscribeToPackage<RequestGroupInformationPackage>(HandleRequestGroupInformationPackage);
        Broker.SubscribeToPackage<LeaveGroupPackage>(HandleLeaveGroupPackage);
    }

    private void HandleClientDisconnected(object? o, ClientDisconnectedEventArgs args)
    {
        var connection = client.GetConnectionFromList(args.ConnectionInformation);
        Leave(connection);
    }
    
    private async void HandleForwardingPackage(object? o, PackageReceivedEventArgs args)
    {
        var package = args.ReceivedPackage as ForwardingPackage;
        var targetInfo = package.TargetInformation as IConnectionInformation;
        
        await client.SendJson(package.PackageJson, targetInfo);
    }

    private async void HandleForwardingPackageAll(object? o, PackageReceivedEventArgs args)
    {
        var package = args.ReceivedPackage as ForwardingPackageAll;

        await client.SendJsonToAllConnections(package.PackageJson);
    }

    private async void HandleRequestGroupInformationPackage(object? o, PackageReceivedEventArgs args)
    {
        var connection = client.GetConnectionFromList(args.ConnectionInformation);
        await connection.SendPackage(new GroupInformationPackage(GroupInformation));
    }

    private void HandleLeaveGroupPackage(object? o, PackageReceivedEventArgs args)
    {
        var connection = client.GetConnectionFromList(args.ConnectionInformation);
        Leave(connection);
        fwServer.PlaceConnectionInMenu(connection);
    }
}