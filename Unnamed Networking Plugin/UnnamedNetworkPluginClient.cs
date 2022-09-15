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
    
    public void AddConnection(IPAddress ipAddress)
    {
        throw new NotImplementedException();
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

    public event Action<IPackage>? PackageReceived;

    public event Action<IPAddress>? ConnectionSuccessful;
}