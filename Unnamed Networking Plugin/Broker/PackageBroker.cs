using Unnamed_Networking_Plugin.Interfaces;

namespace Unnamed_Networking_Plugin.Broker;

public class PackageBroker
{
    public static void ConfigureBrokerPackageIds(IEnumerable<IBrokerPackage> brokerPackages)
    {
        
    }

    public static void SubscribeToPackage<IBrokerPackage>(Action<IBrokerPackage> onPackageReceived)
    {
        
    }

    public static void UnSubscribeFromPackage<IBrokerPackage>(Action<IBrokerPackage> onPackageReceived)
    {
        
    }

    public void InvokeSubscribers(IBrokerPackage package)
    {
        
    }
}