using Unnamed_Networking_Plugin.Interfaces;
using Unnamed_Networking_Plugin.Resources;

namespace ForwardingServer.Resources.InformationPackages;

public class ClientLeftGroupPackage<T> : Package
    where T : IConnectionInformation
{
    public T ConnectionInformation { get; init; }

    public ClientLeftGroupPackage(T connectionInformation)
    {
        ConnectionInformation = connectionInformation;
    }
    
    public IConnectionInformation GetClientInformation()
    {
        return ConnectionInformation;
    }
}