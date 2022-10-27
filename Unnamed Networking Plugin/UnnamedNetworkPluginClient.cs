using System.Net;
using System.Net.Sockets;
using Unnamed_Networking_Plugin.Interfaces;

namespace Unnamed_Networking_Plugin;

public class UnnamedNetworkPluginClient
{
    internal ILogger logger;
    internal IJsonSerializer jsonSerializer;

    private readonly int port;
    private int nextConnectionId = 0;
    private Listener listener;
    private Type connectionIdentificationType;

    public IdentificationPackage identificationPackage { get; private set; }

    private Dictionary<IConnectionInformation, Connection> Connections = new();
    
    public event EventHandler<PackageReceivedEventDetailedArgs>? PackageReceived;

    public event EventHandler<ConnectionReceivedEventArgs>? ConnectionSuccessful;

    public event EventHandler<ClientDisconnectedEventDetailedArgs>? ConnectionLost; 

    public UnnamedNetworkPluginClient(int port, ILogger logger, IJsonSerializer jsonSerializer, IdentificationPackage identificationPackage)
    {
        this.port = port;
        this.logger = logger;
        this.jsonSerializer = jsonSerializer;
        this.identificationPackage = identificationPackage;
        
        connectionIdentificationType = identificationPackage.GetType();
        listener = new Listener(this, port, logger);
        listener.Start();
    }

    public async Task StopListener()
    {
        await listener.Stop();
    }

    public async Task<bool> AddConnection(IPAddress ipAddress, int targetPort)
    {
        var tcpClient = new TcpClient();
        try
        {
            await tcpClient.ConnectAsync(ipAddress, targetPort);
        }
        catch (Exception e)
        {
            logger.Log(this, @$"An error occured when attempting to connect to {ipAddress}:{targetPort} 
{e}", LogType.HandledError);
            return false;
        }

        var connection = new Connection(tcpClient ,tcpClient.GetStream(), jsonSerializer, logger);

        temporarySignal = new SemaphoreSlim(0, 1);
        temporaryRemoteIdentificationPackage = null;

        connection.PackageReceived += GatherIdentificationPackage;
        
        var timeout = Timeout();
        var signalListener = SignalListener(temporarySignal);

        // No need to await this...
        // Or maybe we want to await it at the end of this method to make sure the receiver also added us as a connection?
        connection.SendPackage(identificationPackage);

        Task.WaitAny(timeout, signalListener);
        
        connection.PackageReceived -= GatherIdentificationPackage;
        
        if (signalListener.IsCompleted)
        {
            if (temporaryRemoteIdentificationPackage == null)
            {
                logger.Log(this, "Received identification package was invalid. Disconnecting...", LogType.HandledError);
                return false;
            }

            var remoteInformation = temporaryRemoteIdentificationPackage.ExtractConnectionInformation();

            if (remoteInformation == null)
            {
                logger.Log(this, "Received information was null. Please check your identification package class.", LogType.HandledError);
                return false;
            }

            logger.Log(this, $"Received connection from {remoteInformation}", LogType.Information);
            connection.ConnectionInformation = remoteInformation;
            AddConnectionToList(connection);
            return true;
        }
        
        logger.Log(this, "Remote did not provide identification. Disconnecting...", LogType.HandledError);
        return false;
    }
    
    private SemaphoreSlim temporarySignal;
    private IdentificationPackage? temporaryRemoteIdentificationPackage;
    
    private void GatherIdentificationPackage(object? sender, PackageReceivedEventArgs args)
    {
        temporaryRemoteIdentificationPackage = args.ReceivedPackage as IdentificationPackage;
        temporarySignal.Release();
    }

    /// <summary>
    /// Timeout task. Completes after the constant delay has passed.
    /// </summary>
    private static async Task Timeout()
    {
        await Task.Delay(1000);
    }

    /// <summary>
    /// Listener task. Completes once the provided SemaphoreSlim gets released.
    /// </summary>
    /// <param name="signal">Signal to listen for.</param>
    private static async Task SignalListener(SemaphoreSlim signal)
    {
        await signal.WaitAsync();
    }

    internal void AddConnectionToList(Connection connection)
    {
        Connections.Add(connection.ConnectionInformation, connection);
        connection.PackageReceived += (o, args) =>
        {
            PackageReceived?.Invoke(o, new PackageReceivedEventDetailedArgs(args.ReceivedPackage, args.PackageType, IPAddress.Loopback));
        };
        connection.ClientDisconnected += (o, args) =>
        {
            ConnectionLost?.Invoke(o, new ClientDisconnectedEventDetailedArgs(args.RemoteDisconnected, connection.ConnectionInformation));
        };
        ConnectionSuccessful?.Invoke(this, new ConnectionReceivedEventArgs(connection.ConnectionInformation, connection));
    }



    public Connection GetConnectionFromList(IConnectionInformation targetConnectionInformation)
    {
        return Connections[targetConnectionInformation];
    }

    public async Task RemoveConnection(IConnectionInformation targetConnectionInformation)
    {
        await Connections[targetConnectionInformation].Disconnect();
    }

    public async Task SendPackage<T>(T package, IConnectionInformation targetConnectionInformation)
        where T : IPackage
    {
        await Connections[targetConnectionInformation].SendPackage(package);
    }

    public async Task SendPackageToAllConnections<T>(T package)
    where T : IPackage
    {
        var sendTasks = Connections.Select(connectionEntry => connectionEntry.Value.SendPackage(package)).ToArray();
        await Task.WhenAll(sendTasks);
    }
}

public class ClientDisconnectedEventDetailedArgs : ClientDisconnectedEventArgs
{
    public ClientDisconnectedEventDetailedArgs(bool remoteDisconnected, IConnectionInformation connectionInformation) : base(remoteDisconnected)
    {
        ConnectionInformation = connectionInformation;
    }

    public IConnectionInformation ConnectionInformation { get; }
}

/// <summary>
/// Event information for successful connections.
/// </summary>
public class ConnectionReceivedEventArgs
{
    public ConnectionReceivedEventArgs(IConnectionInformation connectionInformation, Connection connection)
    {
        ConnectionInformation = connectionInformation;
        Connection = connection;
    }
    
    public IConnectionInformation ConnectionInformation { get; }
    public Connection Connection { get; }
}

/// <summary>
/// Event information for received packages and their source.
/// </summary>
public class PackageReceivedEventDetailedArgs : PackageReceivedEventArgs
{
    public PackageReceivedEventDetailedArgs(IPackage receivedPackage, Type packageType, IPAddress senderIpAddress) : base(receivedPackage, packageType)
    {
        SenderIpAddress = senderIpAddress;
    }
    
    public IPAddress SenderIpAddress { get; }
}