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

    private Dictionary<int, Connection> Connections = new();
    
    public event EventHandler<PackageReceivedEventDetailedArgs>? PackageReceived;

    public event EventHandler<ConnectionReceivedEventArgs>? ConnectionSuccessful;

    public UnnamedNetworkPluginClient(int port, ILogger logger, IJsonSerializer jsonSerializer)
    {
        this.port = port;
        this.logger = logger;
        this.jsonSerializer = jsonSerializer;
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
        AddConnectionToList(connection);

        return true;
    }

    internal void AddConnectionToList(Connection connection)
    {
        Connections.Add(nextConnectionId++, connection);
        connection.PackageReceived += (o, args) =>
        {
            PackageReceived?.Invoke(o, new PackageReceivedEventDetailedArgs(args.ReceivedPackage, IPAddress.Loopback));
        };
        ConnectionSuccessful?.Invoke(this, new ConnectionReceivedEventArgs(IPAddress.Loopback));
    }



    public Connection GetConnectionFromList(IPAddress ipAddress)
    {
        throw new NotImplementedException();
    }

    public void RemoveConnection(IPAddress connectionIp)
    {
        throw new NotImplementedException();
    }

    public void SendPackage<T>(T package, IPAddress connectionIp)
    where T : IPackage
    {
        // PackageReceived?.Invoke(this, new PackageReceivedEventArgs(package, IPAddress.Loopback));
        throw new NotImplementedException();
    }

    public async Task SendPackageToAllConnections<T>(T package)
    where T : IPackage
    {
        var sendTasks = Connections.Select(connectionEntry => connectionEntry.Value.SendPackage(package)).ToArray();
        await Task.WhenAll(sendTasks);
    }
}


/// <summary>
/// Event information for successful connections.
/// </summary>
public class ConnectionReceivedEventArgs
{
    public ConnectionReceivedEventArgs(IPAddress connectionIpAddress)
    {
        ConnectionIpAddress = connectionIpAddress;
    }
    
    public IPAddress ConnectionIpAddress { get; }
}

/// <summary>
/// Event information for received packages and their source.
/// </summary>
public class PackageReceivedEventDetailedArgs : PackageReceivedEventArgs
{
    public PackageReceivedEventDetailedArgs(IPackage receivedPackage, IPAddress senderIpAddress) : base(receivedPackage)
    {
        SenderIpAddress = senderIpAddress;
    }
    
    public IPAddress SenderIpAddress { get; }
}