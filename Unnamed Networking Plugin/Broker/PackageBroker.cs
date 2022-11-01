using Unnamed_Networking_Plugin.EventArgs;
using Unnamed_Networking_Plugin.Interfaces;
using Unnamed_Networking_Plugin.Resources;

namespace Unnamed_Networking_Plugin.Broker;

public class PackageBroker
{
    // private Action<IBrokerPackage>[] listeners;
    private readonly Dictionary<Type, EventHandler<PackageReceivedEventDetailedArgs>?> listeners = new();
    public EventHandler<PackageReceivedEventDetailedArgs>? PackageWithNoSubscribersReceived;

    // public static void ConfigureBrokerPackageIds(IEnumerable<IBrokerPackage> brokerPackages)
    // {
    //     
    // }
    
    // TODO. Look into making this class more efficient.

    public void SubscribeToPackage<T>(EventHandler<PackageReceivedEventDetailedArgs> onPackageReceived) where T : Package
    {
        if (listeners.TryGetValue(typeof(T), out var listener))
        {
            listeners[typeof(T)] += onPackageReceived;
            return;
        }
        listeners[typeof(T)] = onPackageReceived;
    }

    public void UnSubscribeFromPackage<T>(EventHandler<PackageReceivedEventDetailedArgs> onPackageReceived) where T : Package
    {
        if (listeners.TryGetValue(typeof(T), out var listener))
        {
            listeners[typeof(T)] -= onPackageReceived;
        }
    }

    public void InvokeSubscribers(PackageReceivedEventDetailedArgs packageReceivedEventArgs)
    {
        if (listeners.TryGetValue(packageReceivedEventArgs.PackageType, out var listener))
        {
            listener?.Invoke(this, packageReceivedEventArgs);
            return;
        }
        PackageWithNoSubscribersReceived?.Invoke(this, packageReceivedEventArgs);
    }
}