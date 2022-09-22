using System.Net;
using Unnamed_Networking_Plugin.Interfaces;

namespace Unnamed_Networking_Plugin;

public class UnnamedNetworkPluginClient
{
    private readonly int port;

    public UnnamedNetworkPluginClient(int port)
    {
        this.port = port;
    }
    
    public async Task<bool> AddConnection(IPAddress ipAddress)
    {
        Console.WriteLine("Attempting connection...");
        await Task.Delay(250);
        ConnectionSuccessful?.Invoke(this, new ConnectionReceivedEventArgs(IPAddress.Loopback));
        Console.WriteLine("Connection successful!");
        return true;
        // throw new NotImplementedException();
    }

    public void RemoveConnection(IPAddress connectionIp)
    {
        throw new NotImplementedException();
    }

    public void SendPackage(IPackage package, IPAddress connectionIp)
    {
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