namespace ForwardingServer;

public record GroupSettings(int MaxSize, string Title, string Description)
{
    public virtual bool Equals(GroupSettings? other)
    {
        if (other == null)
            return false;
        return Title == other.Title;
    }
}