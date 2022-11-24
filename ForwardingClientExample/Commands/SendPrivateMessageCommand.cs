using ForwardingClient;
using ForwardingClientExample.CommandSystem;
using ForwardingServerExampleShared;
using Unnamed_Networking_Plugin.Interfaces;

namespace ForwardingClientExample.Commands;

public class SendPrivateMessageCommand : ITextCommand
{
    public string CommandName => "tell";
    public string Syntax => "/tell *username* *message*";
    public string? Execute(string commandString)
    {
        var arguments = CommandExtractor.GetArguments(commandString);

        if (arguments.Count != 2)
            return "Invalid arguments. Syntax: " + Syntax;

        var package = new PrivateMessagePackage(arguments[1], userIdentification);

        client.PoolTask(client.SendPackageToGroupMember(package, new UserIdentification(arguments[0])));
        return null;
    }
    
    private FwClient client;
    private UserIdentification userIdentification;
    
    public SendPrivateMessageCommand(FwClient fwClient, UserIdentification userIdentification)
    {
        client = fwClient;
        this.userIdentification = userIdentification;
    }
}