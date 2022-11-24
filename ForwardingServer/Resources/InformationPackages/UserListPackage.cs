using Unnamed_Networking_Plugin.Interfaces;
using Unnamed_Networking_Plugin.Resources;

namespace ForwardingServer.Resources.InformationPackages;

public class UserListPackage<T> : Package
where T : IConnectionInformation
{
    public List<T> Users { get; init; }
    
    public UserListPackage(List<T> users)
    {
        Users = users;
    }
}