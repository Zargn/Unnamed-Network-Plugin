using Unnamed_Networking_Plugin.Interfaces;

namespace Network_Plugin_User_frontend;

public class PortIdentifier : IConnectionInformation
{
    public PortIdentifier(int port)
    {
        Port = port;
    }

    public int Port { get; }

    
    // TODO: Can this method be done in a better way?
    public override bool Equals(object? obj)
    {
        if (obj is PortIdentifier)
            return (obj as PortIdentifier).Port == Port;
        return false;
    }
}