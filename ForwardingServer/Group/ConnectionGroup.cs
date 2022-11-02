using Unnamed_Networking_Plugin;
using Unnamed_Networking_Plugin.Broker;

namespace ForwardingServer;

public class ConnectionGroup
{
    public GroupInformation GroupInformation => new GroupInformation()
    public PackageBroker Broker { get; } = new();

    private readonly GroupSettings groupSettings;
    private List<Connection> members = new();

    public ConnectionGroup(GroupSettings groupSettings)
    {
        this.groupSettings = groupSettings;
    }

    public bool Join(Connection connection)
    {
        
    }

    public void Leave(Connection connection)
    {
        
    }
}