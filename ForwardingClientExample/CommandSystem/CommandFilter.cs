using System.Text;

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

        if (commandName == "help")
        {
            StringBuilder CommandListSB = new();
            CommandListSB.AppendLine("Available commands: ");
            int i = 0;
            foreach (var command in commands)
            {
                CommandListSB.AppendLine($"{i++}: {command.Syntax}");
            }

            return CommandListSB.ToString();
        }

        return $"Unknown command: /{commandName}";
    }
}