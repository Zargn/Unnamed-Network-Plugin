using Unnamed_Networking_Plugin;
using Unnamed_Networking_Plugin.Interfaces;
using Unnamed_Networking_Plugin.Resources;

namespace Network_Plugin_User_frontend;

[Serializable]
public class NameIdentifierPackage : IdentificationPackage
{
    public NameIdentifier NameIdentifier;

    public NameIdentifierPackage(NameIdentifier nameIdentifier)
    {
        NameIdentifier = nameIdentifier;
    }

    public override IConnectionInformation? ExtractConnectionInformation()
    {
        return NameIdentifier;
    }
}

[Serializable]
public class NameIdentifier : IConnectionInformation
{
    public string Name { get; init; }
    
    public NameIdentifier(string name)
    {
        Name = name;
    }

    public override bool Equals(object? obj)
    {
        if (obj is NameIdentifier)
            return (obj as NameIdentifier).Name == Name;
        return false;
    }

    public override string ToString()
    {
        return Name;
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
}