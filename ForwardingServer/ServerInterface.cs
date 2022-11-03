using System.Text.RegularExpressions;
using ForwardingServer.Group;
using ForwardingServer.Resources;
using ForwardingServer.Resources.CommandPackages;
using ForwardingServer.Resources.InformationPackages;
using Unnamed_Networking_Plugin;
using Unnamed_Networking_Plugin.Broker;
using Unnamed_Networking_Plugin.EventArgs;
using Unnamed_Networking_Plugin.Resources;

namespace ForwardingServer;

public class ServerInterface
{
    // private List<ConnectionGroup> connectionGroups;
    private Dictionary<GroupSettings, ConnectionGroup> connectionGroups = new();
    private UnnamedNetworkPluginClient client;
    private PackageBroker Broker { get; } = new();


    public ServerInterface(UnnamedNetworkPluginClient client)
    {
        // this.connectionGroups = connectionGroups;
        this.client = client;

        SetUpSubscribers();
    }

    public async Task PutInMenu(Connection connection)
    {
        var sendPackageTask = connection.SendPackage(new InMenuPackage());
        connection.PackageReceived += Broker.InvokeSubscribers;
        connection.ClientDisconnected += HandleClientDisconnected;
        await sendPackageTask;
    }

    public void RemoveFromMenu(Connection connection)
    {
        connection.PackageReceived -= Broker.InvokeSubscribers;
        connection.ClientDisconnected -= HandleClientDisconnected;
    }

    private void SetUpSubscribers()
    {
        Broker.SubscribeToPackage<RequestGroupsListPackage>(HandleListGroupsPackage);
        Broker.SubscribeToPackage<CreateGroupPackage>(HandleCreateGroupPackage);
        Broker.SubscribeToPackage<JoinGroupPackage>(HandleJoinGroupPackage);
    }

    private void HandleClientDisconnected(object? o, ClientDisconnectedEventArgs args)
    {
        var connection = args.Connection;
        RemoveFromMenu(connection);
    }
    
    private async void HandleListGroupsPackage(object? o, PackageReceivedEventArgs args)
    {
        var groupInformationList = connectionGroups.Select(pair => pair.Value.GroupInformation).ToList();

        await client.SendPackage(new GroupsListPackage(groupInformationList), args.ConnectionInformation);
    }

    private async void HandleCreateGroupPackage(object? o, PackageReceivedEventArgs args)
    {
        await client.SendPackage(new RequestGroupSettingsPackage(), args.ConnectionInformation);
        
        
        
        // TODO: Finish this....
        
        
        
    }

    private async void HandleJoinGroupPackage(object? o, PackageReceivedEventArgs args)
    {
        var connection = client.GetConnectionFromList(args.ConnectionInformation);
        RemoveFromMenu(connection);

        var package = args.ReceivedPackage as JoinGroupPackage;

        // Todo: Get group with matching package.TargetGroupInformation then attempt to join.
        try
        {
            var targetGroup = connectionGroups[package.TargetGroupInformation.GroupSettings];
            if (await targetGroup.Join(connection))
            {
                RemoveFromMenu(connection);
                return;
            }
            
            string warningMessage = "Provided group was full.";
            var warningPackage = new WarningPackage(warningMessage, WarningType.GroupFull);
            await connection.SendPackage(warningPackage);
        }
        catch (KeyNotFoundException)
        {
            string warningMessage = "Provided group information does not exist on this server.";
            var warningPackage = new WarningPackage(warningMessage, WarningType.GroupNotFound);
            await connection.SendPackage(warningPackage);
            throw;
        }
        
    }
}