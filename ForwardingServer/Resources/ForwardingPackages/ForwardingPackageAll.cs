using Unnamed_Networking_Plugin.Resources;

namespace ForwardingServer.Resources.ForwardingPackages;

public class ForwardingPackageAll : Package
{
    public string PackageJson { get; init; }
    
    public ForwardingPackageAll(string packageJson)
    {
        PackageJson = packageJson;
    }
}