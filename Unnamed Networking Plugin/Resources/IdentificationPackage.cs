using Unnamed_Networking_Plugin.Interfaces;

namespace Unnamed_Networking_Plugin.Resources;

public abstract class IdentificationPackage : Package
{
    public abstract IConnectionInformation? ExtractConnectionInformation();
}