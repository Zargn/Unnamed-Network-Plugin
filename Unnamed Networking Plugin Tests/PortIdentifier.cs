using Unnamed_Networking_Plugin.Interfaces;

namespace Unnamed_Networking_Plugin_Tests;

public class ClientInformation : IConnectionInformation
{
    public ClientInformation(int port)
    {
        Port = port;
    }

    public int Port { get; }

    
    // TODO: Can this method be done in a better way?
    public override bool Equals(object? obj)
    {
        if (obj is ClientInformation)
            return (obj as ClientInformation).Port == Port;
        return false;
    }
}