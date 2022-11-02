using Unnamed_Networking_Plugin.EventArgs;
using Unnamed_Networking_Plugin.Interfaces;
using Unnamed_Networking_Plugin.Resources;

namespace Unnamed_Networking_Plugin.Broker;

public class PackageBroker
{
    // private Action<IBrokerPackage>[] listeners;
    private readonly Dictionary<Type, EventHandler<PackageReceivedEventArgs>?> listeners = new();
    public EventHandler<PackageReceivedEventArgs>? PackageWithNoSubscribersReceived;

    // public static void ConfigureBrokerPackageIds(IEnumerable<IBrokerPackage> brokerPackages)
    // {
    //     
    // }
    
    // TODO. Look into making this class more efficient.

    public void SubscribeToPackage<T>(EventHandler<PackageReceivedEventArgs> onPackageReceived) where T : Package
    {
        if (listeners.TryGetValue(typeof(T), out var listener))
        {
            listeners[typeof(T)] += onPackageReceived;
            return;
        }
        listeners[typeof(T)] = onPackageReceived;
    }

    public bool SubscribeToPackage(EventHandler<PackageReceivedEventArgs> onPackageReceived, Type packageType)
    {
        if (!packageType.IsSubclassOf(typeof(Package)))
        {
            return false;
        }

        if (listeners.TryGetValue(packageType, out var listener))
        {
            listeners[packageType] += onPackageReceived;
            return true;
        }
        listeners[packageType] = onPackageReceived;
        
        return true;
    }

    public void UnSubscribeFromPackage<T>(EventHandler<PackageReceivedEventArgs> onPackageReceived) where T : Package
    {
        if (listeners.TryGetValue(typeof(T), out var listener))
        {
            listeners[typeof(T)] -= onPackageReceived;
        }
    }

    public bool UnSubscribeFromPackage(EventHandler<PackageReceivedEventArgs> onPackageReceived,
        Type packageType)
    {
        if (!packageType.IsSubclassOf(typeof(Package)))
        {
            return false;
        }

        if (!listeners.TryGetValue(packageType, out var listener))
        {
            return false;
        }
        
        listeners[packageType] -= onPackageReceived;
        return true;

    }

    public void InvokeSubscribers(object? o, PackageReceivedEventArgs packageReceivedEventArgs)
    {
        if (listeners.TryGetValue(packageReceivedEventArgs.PackageType, out var listener))
        {
            listener?.Invoke(this, packageReceivedEventArgs);
            return;
        }
        PackageWithNoSubscribersReceived?.Invoke(this, packageReceivedEventArgs);
    }
}