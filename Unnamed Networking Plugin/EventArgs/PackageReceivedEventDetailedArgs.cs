using Unnamed_Networking_Plugin.Interfaces;

namespace Unnamed_Networking_Plugin.EventArgs;

/// <summary>
/// Event information for received packages and their source.
/// </summary>
public class PackageReceivedEventDetailedArgs : PackageReceivedEventArgs
{
    public PackageReceivedEventDetailedArgs(IPackage receivedPackage, Type packageType, IConnectionInformation connectionInformation) : base(receivedPackage, packageType)
    {
        ConnectionInformation = connectionInformation;
    }

    public IConnectionInformation ConnectionInformation;
}