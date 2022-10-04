using Unnamed_Networking_Plugin.Interfaces;

namespace Unnamed_Networking_Plugin;

public class Package : IPackage
{
    public string Type { get; init; }

    public Package()
    {
        Type = GetType().FullName;
    }
}