using System.Net;
using ForwardingClient.Resources;
using Unnamed_Networking_Plugin;
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
    private bool inMenu;
    
    public bool Connected { get; private set; }
    
    // TODO: This class doesn't really need the entire UnnamedNetworkPluginClient. If we move the authentication
    // to Connection.cs then we should be able to only use one Connection instance for everything.
    
    public FwClient(ILogger logger, IJsonSerializer jsonSerializer, IdentificationPackage identificationPackage)
    {
        Port = Port;
        Logger = logger;
        JsonSerializer = jsonSerializer;
        IdentificationPackage = identificationPackage;
    }
    
    

    public async Task ConnectAsync(IPAddress ipAddress, int port)
    {
        throw new NotImplementedException();
    }

    public async Task DisconnectAsync()
    {
        throw new NotImplementedException();
    }

    public async Task SendListGroupsRequest()
    {
        throw new NotImplementedException();
    }

    public async Task SendCreateGroupRequest(GroupSettings groupSettings)
    {
        throw new NotImplementedException();
    }

    public async Task SendCreateGroupRequest(int maxSize, string title, string description)
    {
        await SendCreateGroupRequest(new GroupSettings(maxSize, title, description));
    }

    public async Task SendJoinGroupRequest(GroupSettings groupSettings)
    {
        throw new NotImplementedException();
    }

    public async Task SendJoinGroupRequest(int maxSize, string title, string description)
    {
        await SendJoinGroupRequest(new GroupSettings(maxSize, title, description));
    }

    public async Task SendPackageToGroupMember<T>(T package, IConnectionInformation connectionInformation)
        where T : Package
    {
        throw new NotImplementedException();
    }

    public async Task SendPackageToAllGroupMembers<T>(T package) where T : Package
    {
        throw new NotImplementedException();
    }

    public async Task SendGroupInformationRequest()
    {
        throw new NotImplementedException();
    }

    public async Task SendLeaveGroupRequest()
    {
        throw new NotImplementedException();
    }
}