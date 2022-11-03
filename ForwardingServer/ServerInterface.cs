using ForwardingServer.Group;
using ForwardingServer.Resources;
using ForwardingServer.Resources.CommandPackages;
using ForwardingServer.Resources.InformationPackages;
using Unnamed_Networking_Plugin;
using Unnamed_Networking_Plugin.Broker;



namespace ForwardingServer;

public class ServerInterface
{
    private Dictionary<GroupSettings, ConnectionGroup> connectionGroups = new();
    private UnnamedNetworkPluginClient client;
    private FwServer FwServer { get; }
    private PackageBroker Broker { get; } = new();


    public ServerInterface(UnnamedNetworkPluginClient client, FwServer fwServer)
    {
        this.client = client;
        FwServer = fwServer;

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
        var connection = client.GetConnectionFromList(args.ConnectionInformation);

        var package = args.ReceivedPackage as CreateGroupPackage;

        var settings = package.GroupSettings;

        if (connectionGroups.ContainsKey(settings))
        {
            string warningMessage = "Provided group settings match a already existing group.";
            var warningPackage = new WarningPackage(warningMessage, WarningType.GroupAlreadyExists);
            await connection.SendPackage(warningPackage);
            return;
        }

        var newGroup = new ConnectionGroup(package.GroupSettings, client, FwServer);
        
        var joinGroupTask = newGroup.Join(connection);

        connectionGroups[settings] = newGroup;
        RemoveFromMenu(connection);

        await joinGroupTask;
    }

    private async void HandleJoinGroupPackage(object? o, PackageReceivedEventArgs args)
    {
        var connection = client.GetConnectionFromList(args.ConnectionInformation);
        RemoveFromMenu(connection);

        var package = args.ReceivedPackage as JoinGroupPackage;
        
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