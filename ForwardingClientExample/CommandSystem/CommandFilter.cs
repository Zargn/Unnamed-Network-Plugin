namespace ForwardingClientExample.CommandSystem;

public class CommandFilter
{
    private ITextCommand[] commands;
    
    public CommandFilter(ITextCommand[] commands)
    {
        this.commands = commands;
    }
    
    public void SetCommandList(ITextCommand[] commands)
    {
        this.commands = commands;
    }

    public string? TryFindAndExecuteCommand(string commandString)
    {
        var commandName = CommandExtractor.GetCommandName(commandString);
        
        foreach (var command in commands)
        {
            if (command.CommandName == commandName)
            {
                return command.Execute(commandString);
            }
        }

        return $"Unknown command: /{commandName}";
    }
}