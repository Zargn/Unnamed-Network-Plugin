using Unnamed_Networking_Plugin.Resources;

namespace Unnamed_Networking_Plugin.Broker;

public class PackageBroker
{
    // private Action<IBrokerPackage>[] listeners;
    private readonly Dictionary<Type, Action<Package>?> listeners = new();
    public EventHandler<Package>? PackageWithNoSubscribersReceived;

    // public static void ConfigureBrokerPackageIds(IEnumerable<IBrokerPackage> brokerPackages)
    // {
    //     
    // }
    
    // TODO. Look into making this class more efficient.

    public void SubscribeToPackage<T>(Action<Package> onPackageReceived) where T : Package
    {
        if (listeners.TryGetValue(typeof(T), out var listener))
        {
            listeners[typeof(T)] += onPackageReceived;
            return;
        }
        listeners[typeof(T)] = onPackageReceived;
    }

    public void UnSubscribeFromPackage<T>(Action<Package> onPackageReceived) where T : Package
    {
        if (listeners.TryGetValue(typeof(T), out var listener))
        {
            listeners[typeof(T)] -= onPackageReceived;
        }
    }

    public void InvokeSubscribers(Package package, Type type)
    {
        if (listeners.TryGetValue(type, out var listener))
        {
            listener?.Invoke(package);
            return;
        }
        PackageWithNoSubscribersReceived?.Invoke(this, package);
    }
}