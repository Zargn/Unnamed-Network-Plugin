using Unnamed_Networking_Plugin.Interfaces;

namespace Unnamed_Networking_Plugin;

public class IdentificationPackage : Package
{
    public IConnectionInformation ConnectionInformation;

    public IdentificationPackage(IConnectionInformation connectionInformation)
    {
        ConnectionInformation = connectionInformation;
    }
}