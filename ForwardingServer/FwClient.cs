using System.Net;
using ForwardingServer;
using ForwardingServer.Resources.CommandPackages;
using ForwardingServer.Resources.ForwardingPackages;
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


    public bool Connected { get; private set; }
    public bool InMenu { get; private set; }
    public bool InGroup { get; private set; }
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
        
        PackageBroker.SubscribeToPackage<InGroupPackage>(HandleInGroupPackage);
        PackageBroker.SubscribeToPackage<InMenuPackage>(HandleInMenuPackage);

        // Todo: Do I need to await or save this somehow to make sure I get the errors?
        // executeTaskPoolTask = ExecuteTaskPool();
    }
    


    public async Task<bool> ConnectAsync(IPAddress ipAddress, int port)
    {
        // throw new NotImplementedException();
        if (Connected)
        {
            Logger.Log(this, $"Client is already connected. Please call disconnect before calling connect again.", LogType.Warning);
            return false;
        }

        Client = new UnnamedNetworkPluginClient(Logger, JsonSerializer, IdentificationPackage);
        
        // TODO: This is only here until a way to start the Client without the listener active is created.
        // await Client.StopListener();
        
        Client.ConnectionSuccessful += HandleConnectionSuccessful;

        var connectTask = Client.AddConnection(ipAddress, port);
        
        return await connectTask;
    }

    private void HandleConnectionSuccessful(object? o, ConnectionReceivedEventArgs args)
    {
        Connected = true;
        
        args.Connection.PackageReceived += PackageBroker.InvokeSubscribers;
        args.Connection.ClientDisconnected += HandleClientDisconnected;

        Connection = args.Connection;
        
        ClientConnected?.Invoke(this, EventArgs.Empty);
    }

    private void HandleClientDisconnected(object? o, ClientDisconnectedEventArgs args)
    {
        InGroup = false;
        InMenu = false;
        Connected = false;
        ClientDisconnected?.Invoke(this, EventArgs.Empty);
    }

    private void HandleInGroupPackage(object? o, PackageReceivedEventArgs args)
    {
        InGroup = true;
        InMenu = false;
    }
    
    private void HandleInMenuPackage(object? o, PackageReceivedEventArgs args)
    {
        InMenu = true;
        InGroup = false;
    }


    private SemaphoreSlim signal = new(0, 1);
    private List<Task> TaskPool = new();

    public void PoolTask(Task taskToPool)
    {
        lock (TaskPool)
        {
            TaskPool.Add(taskToPool);
        }

        signal.Release();
    }

    public async Task GetTaskPoolTask(CancellationToken token)
    {
        while (true)
        {
            try
            {
                await signal.WaitAsync(token);

                Task[] tasks;

                lock (TaskPool)
                {
                    tasks = TaskPool.ToArray();
                    TaskPool.Clear();
                }

                await Task.WhenAll(tasks);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception e)
            {
                Logger.Log(this, $"Pooled task threw error: {e}", LogType.Warning);
            }
        }

    }
    
    public async Task DisconnectAsync()
    {
        Connection.Disconnect();
        // Todo: Add "Connection = null;" ?
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

    public async Task SendPackageToGroupMember<T, TConnectionInformationType>(T package, TConnectionInformationType connectionInformation)
        where T : Package where TConnectionInformationType : IConnectionInformation
    {
        var packageJson = JsonSerializer.Serialize(package);
        await Connection.SendPackage(new ForwardingPackage<TConnectionInformationType>(packageJson, (TConnectionInformationType)connectionInformation));
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

    public async Task SendUserListRequest()
    {
        await Connection.SendPackage(new RequestUserListPackage());
    }

    public async Task SendLeaveGroupRequest()
    {
        await Connection.SendPackage(new LeaveGroupPackage());
    }
}