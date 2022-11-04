namespace ForwardingClientExample.CommandSystem;

public interface ITextCommand
{
    /// <summary>
    /// Command name.
    /// </summary>
    public string CommandName { get; }

    /// <summary>
    /// Execute the command.
    /// </summary>
    /// <param name="commandString">full command string.</param>
    /// <returns>Potential output string.</returns>
    public string? Execute(string commandString);
}