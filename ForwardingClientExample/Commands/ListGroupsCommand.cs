using ForwardingClientExample.CommandSystem;

namespace ForwardingClientExample.Commands;



public class ListGroupsCommand : ITextCommand
{
    public string CommandName => "list";
    public string Syntax => "/list";

    public string? Execute(string commandString)
    {
        var arguments = CommandExtractor.GetArguments(commandString);
        
        foreach (var s in arguments)
        {
            Console.WriteLine(s);
        }
        
        return "GroupList: null";
    }
}