namespace Unnamed_Networking_Plugin.Interfaces;

public interface ILogger
{
    public void Log(object sender, string message, LogType logType);
}

public enum LogType
{
    Error,
    Warning,
    HandledError,
    Information
}