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

    public UnnamedNetworkPluginClient(int port, ILogger logger, IJsonSerializer jsonSerializer)
    {
        this.port = port;
        this.logger = logger;
        this.jsonSerializer = jsonSerializer;
        listener = new Listener(this, port);
        listener.Start();
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
            Console.WriteLine($"An error occured when attempting to connect to {ipAddress}:{targetPort}");
            throw;
        }

        var connection = new Connection(tcpClient ,tcpClient.GetStream(), jsonSerializer, logger);
        AddConnectionToList(connection);
        
        
        // TODO: The connection class should maybe be the one with the Send/Receive package events/methods.
        // Current plan:
        // 1. Finish the connection class. A instance of this class will be created on each successful incoming our
        // outgoing connection. It should contain information about the connection, and the data stream used to send and
        // receive data. 
        
        
        // Todo: This might be useful by making each connection have a "Read" method that uses this following method.
        // Then I can use one while loop in the main class that has a Task.WaitAny() or similar.
        // StreamReader sr = new StreamReader(connection.DataStream);
        // sr.ReadLineAsync();

        return true;
        
        throw new NotImplementedException();
    }

    internal void AddConnectionToList(Connection connection)
    {
        Connections.Add(nextConnectionId++, connection);
        ConnectionSuccessful?.Invoke(this, new ConnectionReceivedEventArgs(IPAddress.Loopback));
    }

    public void RemoveConnection(IPAddress connectionIp)
    {
        throw new NotImplementedException();
    }

    public void SendPackage(IPackage package, IPAddress connectionIp)
    {
        // PackageReceived?.Invoke(this, new PackageReceivedEventArgs(package, IPAddress.Loopback));
        throw new NotImplementedException();
    }

    public void SendPackageToAllConnections(IPackage package)
    {
        throw new NotImplementedException();
    }

    public event EventHandler<PackageReceivedEventArgs>? PackageReceived;

    public event EventHandler<ConnectionReceivedEventArgs>? ConnectionSuccessful;
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
/// Event information for received packages.
/// </summary>
public class PackageReceivedEventArgs
{
    public PackageReceivedEventArgs(IPackage receivedPackage, IPAddress senderIpAddress)
    {
        SenderIpAddress = senderIpAddress;
        ReceivedPackage = receivedPackage;
    }
    
    public IPAddress SenderIpAddress { get; }
    public IPackage ReceivedPackage { get; }
}