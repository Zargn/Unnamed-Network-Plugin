﻿using System.Text.RegularExpressions;
using ForwardingServer.Group;
using ForwardingServer.Resources;
using Unnamed_Networking_Plugin;
using Unnamed_Networking_Plugin.EventArgs;
using Unnamed_Networking_Plugin.Resources;
using PackageReceivedEventArgs = Unnamed_Networking_Plugin.EventArgs.PackageReceivedEventArgs;

namespace ForwardingServer;

public class ServerInterface
{
    private List<ConnectionGroup> connectionGroups;
    private UnnamedNetworkPluginClient client;

    public ServerInterface(List<ConnectionGroup> connectionGroups, UnnamedNetworkPluginClient client)
    {
        this.connectionGroups = connectionGroups;
        this.client = client;
    }

    public void HandleListGroupsPackage(PackageReceivedEventArgs args)
    {
        List<GroupInformation> groupInformationList = connectionGroups.Select(connectionGroup => connectionGroup.GroupInformation).ToList();
        
        var sendPackageTask = client.SendPackage(new GroupsListPackage(groupInformationList), args.ConnectionInformation);
        
        sendPackageTask.Wait();
    }

    public void HandleCreateGroupPackage(PackageReceivedEventArgs args)
    {
        var sendPackageTask = client.SendPackage(new RequestGroupSettingsPackage(), args.ConnectionInformation);
        
        sendPackageTask.Wait();
    }

    public void HandleJoinGroupPackage(PackageReceivedEventArgs args)
    {
        
    }
}