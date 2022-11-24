namespace ForwardingClientExample.CommandSystem;

public interface ITextCommand
{
    /// <summary>
    /// Command name.
    /// </summary>
    public string CommandName { get; }
 
    /// <summary>
    /// Command syntax including format of arguemnts.
    /// </summary>
    public string Syntax { get; }

    /// <summary>
    /// Execute the command.
    /// </summary>
    /// <param name="commandString">full command string.</param>
    /// <returns>Potential output string.</returns>
    public string? Execute(string commandString);
}