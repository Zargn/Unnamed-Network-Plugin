using Unnamed_Networking_Plugin.Interfaces;
using Unnamed_Networking_Plugin.Resources;

namespace ForwardingServer.Resources.InformationPackages;

public class ClientLeftGroupPackage : Package
{
    public object ConnectionInformation { get; init; }

    public ClientLeftGroupPackage(IConnectionInformation connectionInformation)
    {
        ConnectionInformation = connectionInformation;
    }
}