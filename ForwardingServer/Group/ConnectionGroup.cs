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

    public ConnectionGroup(GroupSettings groupSettings, Type forwardingPackageType, UnnamedNetworkPluginClient client)
    {
        this.groupSettings = groupSettings;
        this.forwardingPackageType = forwardingPackageType;
        this.client = client;

        SetUpSubscribers();
    }

    public bool Join(Connection connection)
    {
        if (MemberCount >= groupSettings.MaxSize)
            return false;

        // connection.PackageReceived += Broker.InvokeSubscribers;
        
        return true;
    }

    public void Leave(Connection connection)
    {
        
    }

    private void SetUpSubscribers()
    {
        Broker.SubscribeToPackage<LeaveGroupPackage>(HandleLeaveGroupPackage);
        Broker.SubscribeToPackage(HandleForwardingPackage, forwardingPackageType);
        Broker.SubscribeToPackage<RequestGroupInformationPackage>(HandleRequestGroupInformationPackage);
        Broker.SubscribeToPackage<LeaveGroupPackage>(HandleLeaveGroupPackage);
    }
    
    private void HandleForwardingPackage(object? o, PackageReceivedEventArgs args)
    {
        var package = args.ReceivedPackage as ForwardingPackage;
        var targetInfo = package.TargetInformation as IConnectionInformation;

        // Todo: Send package.PackageJson to targetInfo
        client.SendJson(package.PackageJson, targetInfo);
    }

    private void ForwardPackageToAll(object? o, PackageReceivedEventArgs args)
    {
        
    }

    private void HandleRequestGroupInformationPackage(object? o, PackageReceivedEventArgs args)
    {
        
    }

    private void HandleLeaveGroupPackage(object? o, PackageReceivedEventArgs args)
    {
        
    }
}