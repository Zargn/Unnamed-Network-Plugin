using Unnamed_Networking_Plugin.Resources;

namespace ForwardingServer.Resources.CommandPackages;

public class JoinGroupPackage : Package
{
    public GroupSettings TargetGroupSettings { get; init; }
    
    public JoinGroupPackage(GroupSettings targetGroupSettings)
    {
        TargetGroupSettings = targetGroupSettings;
    }
}