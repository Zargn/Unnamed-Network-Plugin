using Unnamed_Networking_Plugin.Resources;

namespace ForwardingServer.Resources.InformationPackages;

public class GroupSettingsPackage : Package
{
    public GroupSettings GroupSettings { get; init; }

    public GroupSettingsPackage(GroupSettings groupSettings)
    {
        GroupSettings = groupSettings;
    }
}