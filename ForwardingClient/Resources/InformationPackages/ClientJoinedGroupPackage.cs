using Unnamed_Networking_Plugin.Interfaces;
using Unnamed_Networking_Plugin.Resources;

namespace ForwardingClient.Resources.InformationPackages;

public class ClientJoinedGroupPackage : Package
{
    public object ClientInformation { get; init; }

    public ClientJoinedGroupPackage(IConnectionInformation clientInformation)
    {
        ClientInformation = clientInformation;
    }
}