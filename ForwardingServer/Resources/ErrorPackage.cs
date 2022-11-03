using Unnamed_Networking_Plugin.Resources;

namespace ForwardingServer.Resources;

public class ErrorPackage : Package
{
    public string ErrorMessage { get; init; }
    public Exception Exception { get; init; }

    public ErrorPackage(string errorMessage, Exception exception)
    {
        ErrorMessage = errorMessage;
        Exception = exception;
    }
}