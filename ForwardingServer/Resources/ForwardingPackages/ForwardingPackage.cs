using Unnamed_Networking_Plugin.Resources;

namespace ForwardingServer.Resources.ForwardingPackages;

/// <summary>
/// Forwarding Package class. Wraps a pre-made json string in a package together with targetInformation for the
/// destination.
/// </summary>
public class ForwardingPackage : Package
{
    public object TargetInformation { get; init; }
    
    public string PackageJson { get; init; }
    
    public ForwardingPackage(string packageJson, object targetInformation)
    {
        PackageJson = packageJson;
        TargetInformation = targetInformation;
    }
}