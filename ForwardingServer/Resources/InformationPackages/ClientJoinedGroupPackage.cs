using Unnamed_Networking_Plugin.Interfaces;
using Unnamed_Networking_Plugin.Resources;

namespace ForwardingServer.Resources.InformationPackages;

public class ClientJoinedGroupPackage<T> : Package
where T : IConnectionInformation
{
    public T ClientInformation { get; init; }

    public ClientJoinedGroupPackage(T clientInformation)
    {
        ClientInformation = clientInformation;
    }

    public IConnectionInformation GetClientInformation()
    {
        return ClientInformation;
    }
}