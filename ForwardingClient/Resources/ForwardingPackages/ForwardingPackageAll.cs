using Unnamed_Networking_Plugin.Resources;

namespace ForwardingClient.Resources.ForwardingPackages;

public class ForwardingPackageAll : Package
{
    public string PackageJson { get; init; }
    
    public ForwardingPackageAll(string packageJson)
    {
        PackageJson = packageJson;
    }
}