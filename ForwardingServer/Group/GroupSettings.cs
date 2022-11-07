namespace ForwardingServer;

public class GroupSettings
{
    public GroupSettings(int maxSize, string title, string description)
    {
        MaxSize = maxSize;
        Title = title;
        Description = description;
    }

    public int MaxSize { get; init; }
    public string Title { get; init; }
    public string Description { get; init; }

    public override bool Equals(object? obj)
    {
        if (obj is GroupSettings settings)
            return Title == settings.Title;
        return false;
    }

    public override int GetHashCode()
    {
        return Title.GetHashCode();
    }

    public override string ToString()
    {
        return $"{Title} | {Description} | {MaxSize}";
    }
}