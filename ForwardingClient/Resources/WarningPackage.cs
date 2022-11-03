using Unnamed_Networking_Plugin.Resources;

namespace ForwardingClient.Resources;

public class WarningPackage : Package
{
    public string WarningMessage { get; init; }
    public WarningType WarningType { get; init; }
    
    public WarningPackage(string warningMessage, WarningType warningType)
    {
        WarningMessage = warningMessage;
        WarningType = warningType;
    }
}

public enum WarningType
{
    GroupFull,
    GroupNotFound,
    ClientNotFound,
    GroupAlreadyExists
}