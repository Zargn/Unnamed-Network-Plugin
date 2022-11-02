using ForwardingServer.Group.Resources;
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

    public void LeaveGroup(Connection connection)
    {
        connection.PackageReceived -= Broker.InvokeSubscribers;
        connection.ClientDisconnected -= HandleClientDisconnected;

        members.Remove(connection);
    }

    private void SetUpSubscribers()
    {
        Broker.SubscribeToPackage<LeaveGroupPackage>(HandleLeaveGroupPackage);
        Broker.SubscribeToPackage(HandleForwardingPackage, forwardingPackageType);
        Broker.SubscribeToPackage<RequestGroupInformationPackage>(HandleRequestGroupInformationPackage);
        Broker.SubscribeToPackage<LeaveGroupPackage>(HandleLeaveGroupPackage);
    }

    private void HandleClientDisconnected(object? o, ClientDisconnectedEventArgs args)
    {
        var connection = client.GetConnectionFromList(args.ConnectionInformation);
        LeaveGroup(connection);
    }
    
    private void HandleForwardingPackage(object? o, PackageReceivedEventArgs args)
    {
        var package = args.ReceivedPackage as ForwardingPackage;
        var targetInfo = package.TargetInformation as IConnectionInformation;

        // Todo: Send package.PackageJson to targetInfo
        var sendJsonTask = client.SendJson(package.PackageJson, targetInfo);
        sendJsonTask.Wait();
    }

    private void ForwardPackageToAll(object? o, PackageReceivedEventArgs args)
    {
        
    }

    private void HandleRequestGroupInformationPackage(object? o, PackageReceivedEventArgs args)
    {
        
    }

    private void HandleLeaveGroupPackage(object? o, PackageReceivedEventArgs args)
    {
        var connection = client.GetConnectionFromList(args.ConnectionInformation);
        LeaveGroup(connection);
        fwServer.PlaceConnectionInMenu(connection);
    }
}