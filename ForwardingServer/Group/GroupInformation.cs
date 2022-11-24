namespace ForwardingServer;

public class GroupInformation
{
    public GroupSettings GroupSettings { get; init; }
    public int MemberCount { get; init; }
    
    public GroupInformation(GroupSettings groupSettings, int memberCount)
    {
        GroupSettings = groupSettings;
        MemberCount = memberCount;
    }

    public override string ToString()
    {
        return $"[{MemberCount}/{GroupSettings.MaxSize}] | {GroupSettings.Title} | {GroupSettings.Description}";
    }
}