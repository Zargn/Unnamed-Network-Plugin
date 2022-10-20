using Unnamed_Networking_Plugin.Interfaces;

namespace Unnamed_Networking_Plugin_Tests;

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