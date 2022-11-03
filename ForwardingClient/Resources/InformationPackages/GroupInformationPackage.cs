using Unnamed_Networking_Plugin.Resources;

namespace ForwardingClient.Resources.InformationPackages;

public class GroupInformationPackage : Package
{
    public GroupInformation GroupInformation { get; init; }
    
    public GroupInformationPackage(GroupInformation groupInformation)
    {
        GroupInformation = groupInformation;
    }

}