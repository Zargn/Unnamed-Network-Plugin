using Unnamed_Networking_Plugin.Resources;

namespace ForwardingServer.Resources.InformationPackages;

public class GroupsListPackage : Package
{
    public List<GroupInformation> GroupInformation { get; init; }
    
    public GroupsListPackage(List<GroupInformation> groupInformation)
    {
        GroupInformation = groupInformation;
    }

}