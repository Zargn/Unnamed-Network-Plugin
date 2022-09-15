using System.Net;
using Unnamed_Networking_Plugin.Interfaces;

namespace Unnamed_Networking_Plugin;

public class UnnamedNetworkPluginClient
{
    public void AddConnection(IPAddress ipAddress, int port)
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
}