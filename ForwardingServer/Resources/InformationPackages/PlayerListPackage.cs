using Unnamed_Networking_Plugin.Interfaces;
using Unnamed_Networking_Plugin.Resources;

namespace ForwardingServer.Resources.InformationPackages;

public class PlayerListPackage<T> : Package
where T : IConnectionInformation
{
    public List<T> Users { get; init; }
    
    public PlayerListPackage(List<T> users)
    {
        Users = users;
    }
}