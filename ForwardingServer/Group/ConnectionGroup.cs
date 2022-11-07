using ForwardingServer.Resources.CommandPackages;
using ForwardingServer.Resources.ForwardingPackages;
using ForwardingServer.Resources.InformationPackages;
using Unnamed_Networking_Plugin;
using Unnamed_Networking_Plugin.Broker;
using Unnamed_Networking_Plugin.Interfaces;
using Unnamed_Networking_Plugin.Resources;


namespace ForwardingServer.Group;

public class ConnectionGroup<TConnectionInformationType>
where TConnectionInformationType : IConnectionInformation
{
    public GroupInformation GroupInformation => new(groupSettings, MemberCount);
    public PackageBroker Broker { get; } = new();
    public int MemberCount => members.Count;
    
    private readonly GroupSettings groupSettings;
    private List<Connection> members = new();
    private UnnamedNetworkPluginClient client;
    private FwServer<TConnectionInformationType> fwServer;

    public ConnectionGroup(GroupSettings groupSettings, UnnamedNetworkPluginClient client, FwServer<TConnectionInformationType> fwServer)
    {
        this.groupSettings = groupSettings;
        this.client = client;
        this.fwServer = fwServer;

        SetUpSubscribers();
    }

    public async Task<bool> Join(Connection connection)
    {
        if (MemberCount >= groupSettings.MaxSize)
            return false;

        var sendPackageTask = connection.SendPackage(new InGroupPackage());

        connection.PackageReceived += Broker.InvokeSubscribers;
        connection.ClientDisconnected += HandleClientDisconnected;

        members.Add(connection);

        await sendPackageTask;
        
        await SendPackageToEveryoneInGroup(new ClientJoinedGroupPackage<TConnectionInformationType>((TConnectionInformationType)connection.ConnectionInformation));
        
        return true;
    }

    public async Task Leave(Connection connection)
    {
        connection.PackageReceived -= Broker.InvokeSubscribers;
        connection.ClientDisconnected -= HandleClientDisconnected;
        
        members.Remove(connection);
        
        await SendPackageToEveryoneInGroup(new ClientLeftGroupPackage(connection.ConnectionInformation));
    }

    private async Task SendPackageToEveryoneInGroup(Package package)
    {
        List<Task> sendTasks = members.Select(connection => connection.SendPackage(package)).ToList();
        await Task.WhenAll(sendTasks);
    }

    private void SetUpSubscribers()
    {
        Broker.SubscribeToPackage<ForwardingPackage>(HandleForwardingPackage);
        Broker.SubscribeToPackage<ForwardingPackageAll>(HandleForwardingPackageAll);
        Broker.SubscribeToPackage<RequestGroupInformationPackage>(HandleRequestGroupInformationPackage);
        Broker.SubscribeToPackage<LeaveGroupPackage>(HandleLeaveGroupPackage);
    }

    private async void HandleClientDisconnected(object? o, ClientDisconnectedEventArgs args)
    {
        var connection = args.Connection;
        await Leave(connection);
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

        List<Task> sendTasks = members.Select(connection => connection.SendJson(package.PackageJson)).ToList();
        await Task.WhenAll(sendTasks);
    }

    private async void HandleRequestGroupInformationPackage(object? o, PackageReceivedEventArgs args)
    {
        var connection = client.GetConnectionFromList(args.ConnectionInformation);
        await connection.SendPackage(new GroupInformationPackage(GroupInformation));
    }

    private async void HandleLeaveGroupPackage(object? o, PackageReceivedEventArgs args)
    {
        var connection = client.GetConnectionFromList(args.ConnectionInformation);
        await Leave(connection);
        fwServer.PlaceConnectionInMenu(connection);
    }
}