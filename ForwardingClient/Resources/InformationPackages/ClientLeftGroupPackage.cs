using Unnamed_Networking_Plugin.Interfaces;
using Unnamed_Networking_Plugin.Resources;

namespace ForwardingClient.Resources.InformationPackages;

public class ClientLeftGroupPackage : Package
{
    public object ConnectionInformation { get; init; }

    public ClientLeftGroupPackage(IConnectionInformation connectionInformation)
    {
        ConnectionInformation = connectionInformation;
    }
}