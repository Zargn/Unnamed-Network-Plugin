using Unnamed_Networking_Plugin;
using Unnamed_Networking_Plugin.Interfaces;
using Unnamed_Networking_Plugin.Packages;

namespace Network_Plugin_User_frontend;

[Serializable]
public class PortIdentifierPackage : IdentificationPackage
{
    public PortIdentifier Identifier { get; init; }
    
    public PortIdentifierPackage(PortIdentifier identifier)
    {
        Identifier = identifier;
    }
    
    public override IConnectionInformation ExtractConnectionInformation()
    {
        return Identifier;
    }
}

[Serializable]
public class PortIdentifier : IConnectionInformation
{
    public int Port { get; init; }
    
    public PortIdentifier(int port)
    {
        Port = port;
    }
    
    // TODO: Can this method be done in a better way?
    public override bool Equals(object? obj)
    {
        if (obj is PortIdentifier)
            return (obj as PortIdentifier).Port == Port;
        return false;
    }

    public override string ToString()
    {
        return Port.ToString();
    }

    public override int GetHashCode()
    {
        return Port.GetHashCode();
    }
}