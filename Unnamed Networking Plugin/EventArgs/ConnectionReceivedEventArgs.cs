using Unnamed_Networking_Plugin.Interfaces;

namespace Unnamed_Networking_Plugin.EventArgs;

/// <summary>
/// Event information for successful connections.
/// </summary>
public class ConnectionReceivedEventArgs
{
    public ConnectionReceivedEventArgs(IConnectionInformation connectionInformation, Connection connection)
    {
        ConnectionInformation = connectionInformation;
        Connection = connection;
    }
    
    public IConnectionInformation ConnectionInformation { get; }
    public Connection Connection { get; }
}
