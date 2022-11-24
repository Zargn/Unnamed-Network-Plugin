using System.Text;
using Unnamed_Networking_Plugin.Interfaces;

namespace ForwardingClientExample;

public class LogFileController : ILogger
{
    private bool printingEnabled;
    private List<string> logList = new();
    
    public LogFileController(bool printingEnabled)
    {
        this.printingEnabled = printingEnabled;
    }
    
    public void Log(object sender, string message, LogType logType)
    {
        lock (logList)
        {
            logList.Add($"[{logType.ToString()}] {sender}: {message}");
        }

        if (printingEnabled)
            Console.WriteLine($"[{logType.ToString()}] {sender}: {message}");
    }

    public string GetLogs()
    {
        StringBuilder sb = new();
        
        lock (logList)
        {
            foreach (var s in logList)
            {
                sb.AppendLine(s);
            }
        }

        return sb.ToString();
    }
    
    // TODO: The logs should be written to a file in the end instead of printed. Maybe.
}