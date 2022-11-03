using System.Net;
using System.Threading.Channels;
using ForwardingClient.Resources;
using ForwardingClient.Resources.CommandPackages;
using ForwardingClient.Resources.ForwardingPackages;
using Unnamed_Networking_Plugin;
using Unnamed_Networking_Plugin.Broker;
using Unnamed_Networking_Plugin.EventArgs;
using Unnamed_Networking_Plugin.Interfaces;
using Unnamed_Networking_Plugin.Resources;



namespace ForwardingClient;

public class FwClient
{
    private int Port { get; set; }
    private ILogger Logger { get; }
    private IJsonSerializer JsonSerializer { get; }
    private IdentificationPackage IdentificationPackage { get; }
    private UnnamedNetworkPluginClient Client { get; set; }
    private Connection Connection { get; set; }
    private bool inMenu;
    
    public bool Connected { get; private set; }
    public PackageBroker PackageBroker { get; }

    public event EventHandler? ClientDisconnected;
    public event EventHandler? ClientConnected;

    // TODO: This class doesn't really need the entire UnnamedNetworkPluginClient. If we move the authentication
    // to Connection.cs then we should be able to only use one Connection instance for everything.
    
    public FwClient(ILogger logger, IJsonSerializer jsonSerializer, IdentificationPackage identificationPackage)
    {
        Logger = logger;
        JsonSerializer = jsonSerializer;
        IdentificationPackage = identificationPackage;
        PackageBroker = new PackageBroker();
    }



    public async Task<bool> ConnectAsync(IPAddress ipAddress, int port)
    {
        // throw new NotImplementedException();

        Client = new UnnamedNetworkPluginClient(port, Logger, JsonSerializer, IdentificationPackage);
        
        // TODO: This is only here until a way to start the Client without the listener active is created.
        var stopListenerTask = Client.StopListener();

        Client.ConnectionSuccessful += HandleConnectionSuccessful;

        var connectTask = Client.AddConnection(ipAddress, port);
        return await connectTask;
    }

    private void HandleConnectionSuccessful(object? o, ConnectionReceivedEventArgs args)
    {
        args.Connection.PackageReceived += PackageBroker.InvokeSubscribers;
        args.Connection.ClientDisconnected += HandleClientDisconnected;

        Connection = args.Connection;
    }

    private void HandleClientDisconnected(object? o, ClientDisconnectedEventArgs args)
    {
        
    }

    
    
    public async Task DisconnectAsync()
    {
        Connection.Disconnect();
    }

    public async Task SendListGroupsRequest()
    {
        await Connection.SendPackage(new RequestGroupsListPackage());
    }

    public async Task SendCreateGroupRequest(GroupSettings groupSettings)
    {
        await Connection.SendPackage(new CreateGroupPackage(groupSettings));
    }

    public async Task SendCreateGroupRequest(int maxSize, string title, string description)
    {
        await SendCreateGroupRequest(new GroupSettings(maxSize, title, description));
    }

    public async Task SendJoinGroupRequest(GroupSettings groupSettings)
    {
        await Connection.SendPackage(new JoinGroupPackage(groupSettings));
    }

    public async Task SendJoinGroupRequest(int maxSize, string title, string description)
    {
        await SendJoinGroupRequest(new GroupSettings(maxSize, title, description));
    }

    public async Task SendPackageToGroupMember<T>(T package, IConnectionInformation connectionInformation)
        where T : Package
    {
        var packageJson = JsonSerializer.Serialize(package);
        await Connection.SendPackage(new ForwardingPackage(packageJson, connectionInformation));
    }

    public async Task SendPackageToAllGroupMembers<T>(T package) where T : Package
    {
        var packageJson = JsonSerializer.Serialize(package);
        await Connection.SendPackage(new ForwardingPackageAll(packageJson));
    }

    public async Task SendGroupInformationRequest()
    {
        await Connection.SendPackage(new RequestGroupInformationPackage());
    }

    public async Task SendLeaveGroupRequest()
    {
        await Connection.SendPackage(new LeaveGroupPackage());
    }
}