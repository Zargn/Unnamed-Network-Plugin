using Unnamed_Networking_Plugin.Resources;

namespace ForwardingServer.Resources.CommandPackages;

public class CreateGroupPackage : Package
{
    public GroupSettings GroupSettings { get; init; }

    public CreateGroupPackage(GroupSettings groupSettings)
    {
        GroupSettings = groupSettings;
    }
}