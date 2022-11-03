using Unnamed_Networking_Plugin.Resources;

namespace ForwardingServer.Resources.CommandPackages;

public class JoinGroupPackage : Package
{
    public GroupInformation TargetGroupInformation { get; init; }
    
    public JoinGroupPackage(GroupInformation targetGroupInformation)
    {
        TargetGroupInformation = targetGroupInformation;
    }
}