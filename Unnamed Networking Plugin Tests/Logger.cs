using Unnamed_Networking_Plugin.Interfaces;

namespace Unnamed_Networking_Plugin_Tests;

public class Logger : ILogger
{
    public void Log(object sender, string message, LogType logType)
    {
        Console.WriteLine($"[{logType.ToString()}] {sender}: {message}");
    }
}