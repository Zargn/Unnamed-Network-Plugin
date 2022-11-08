using ForwardingClientExample.CommandSystem;

namespace ForwardingClientExample.Commands;

public class PrintLogsCommand : ITextCommand
{
    public string CommandName => "printlogs";
    public string Syntax => "/printlogs";
    public string? Execute(string commandString)
    {
        return logFileController.GetLogs();
    }

    private LogFileController logFileController;
    
    public PrintLogsCommand(LogFileController logFileController)
    {
        this.logFileController = logFileController;
    }
}