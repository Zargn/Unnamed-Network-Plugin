using Unnamed_Networking_Plugin.Interfaces;
using Unnamed_Networking_Plugin.Resources;

namespace ForwardingClientExample;

public class UserIdentificationPackage : IdentificationPackage
{
    public UserIdentification UserIdentification { get; init; }

    public UserIdentificationPackage(UserIdentification userIdentification)
    {
        UserIdentification = userIdentification;
    }

    public override IConnectionInformation ExtractConnectionInformation()
    {
        return UserIdentification;
    }
}

public class UserIdentification : IConnectionInformation
{
    public string UserName { get; init; }

    public UserIdentification(string userName)
    {
        UserName = userName;
    }

    public override bool Equals(object? obj)
    {
        if (obj is UserIdentification)
            return (obj as UserIdentification).UserName == UserName;
        return false;
    }

    public override string ToString()
    {
        return UserName;
    }

    public override int GetHashCode()
    {
        return UserName.GetHashCode();
    }
}