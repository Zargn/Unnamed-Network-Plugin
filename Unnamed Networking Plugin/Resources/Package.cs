using Unnamed_Networking_Plugin.Interfaces;

namespace Unnamed_Networking_Plugin.Resources;

[Serializable]
public class Package : IPackage
{
    public string Type { get; init; }

    public Package()
    {
        Type = GetType().FullName;
    }
}