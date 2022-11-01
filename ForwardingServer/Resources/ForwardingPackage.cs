using Unnamed_Networking_Plugin.Interfaces;
using Unnamed_Networking_Plugin.Resources;

namespace ForwardingServer;

/// <summary>
/// Forwarding Package class. Wraps a pre-made json string in a package together with targetInformation for the
/// destination.
/// </summary>
/// <typeparam name="T">IConnectionInformation type.</typeparam>
public class ForwardingPackage<T> : Package where T : IConnectionInformation
{
    public T TargetInformation { get; init; }
    
    public string PackageJson { get; init; }
    
    public ForwardingPackage(string packageJson, T targetInformation)
    {
        PackageJson = packageJson;
        TargetInformation = targetInformation;
    }
}