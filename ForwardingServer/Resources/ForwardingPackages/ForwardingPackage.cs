using Unnamed_Networking_Plugin.Interfaces;
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
    
    public ForwardingPackage(string packageJson, IConnectionInformation targetInformation)
    {
        PackageJson = packageJson;
        TargetInformation = targetInformation;
    }
}

/*
/// <summary>
/// Forwarding Package class. Wraps a pre-made json string in a package together with targetInformation for the
/// destination.
/// </summary>
public class ForwardingPackage<T> : Package
where T : IConnectionInformation
{
    public T TargetInformation { get; init; }
    
    public string PackageJson { get; init; }
    
    public ForwardingPackage(string packageJson, T targetInformation)
    {
        PackageJson = packageJson;
        TargetInformation = targetInformation;
    }

    public IConnectionInformation GetConnectionInformation()
    {
        return TargetInformation;
    }
}



*/