using Unnamed_Networking_Plugin.Interfaces;

namespace ForwardingClientExample;

public class LogFileController : ILogger
{
    public void Log(object sender, string message, LogType logType)
    {
        Console.WriteLine($"[{logType.ToString()}] {sender}: {message}");
    }
    
    // TODO: The logs should be written to a file in the end instead of printed. Maybe.
}