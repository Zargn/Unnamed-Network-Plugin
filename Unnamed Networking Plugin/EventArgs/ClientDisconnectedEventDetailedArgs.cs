using Unnamed_Networking_Plugin.Interfaces;

namespace Unnamed_Networking_Plugin.EventArgs;

/// <summary>
/// Event information for disconnections.
/// </summary>
public class ClientDisconnectedEventDetailedArgs : ClientDisconnectedEventArgs
{
    public ClientDisconnectedEventDetailedArgs(bool remoteDisconnected, IConnectionInformation connectionInformation) : base(remoteDisconnected)
    {
        ConnectionInformation = connectionInformation;
    }

    public IConnectionInformation ConnectionInformation { get; }
}