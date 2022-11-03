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

    public void PutInMenu(Connection connection)
    {
        
    }

    public void RemoveFromMenu(Connection connection)
    {
        
    }

    private void SetUpSubscribers()
    {
        Broker.SubscribeToPackage<RequestGroupsListPackage>(HandleListGroupsPackage);
        Broker.SubscribeToPackage<CreateGroupPackage>(HandleCreateGroupPackage);
        Broker.SubscribeToPackage<JoinGroupPackage>(HandleJoinGroupPackage);
    }

    private async void HandleListGroupsPackage(object? o, PackageReceivedEventArgs args)
    {
        var groupInformationList = connectionGroups.Select(pair => pair.Value.GroupInformation).ToList();

        await client.SendPackage(new GroupsListPackage(groupInformationList), args.ConnectionInformation);
    }

    private async void HandleCreateGroupPackage(object? o, PackageReceivedEventArgs args)
    {
        await client.SendPackage(new RequestGroupSettingsPackage(), args.ConnectionInformation);
    }

    private void HandleJoinGroupPackage(object? o, PackageReceivedEventArgs args)
    {
        var connection = client.GetConnectionFromList(args.ConnectionInformation);
        RemoveFromMenu(connection);

        var package = args.ReceivedPackage as JoinGroupPackage;

        // Todo: Get group with matching package.TargetGroupInformation then attempt to join.
    }
}