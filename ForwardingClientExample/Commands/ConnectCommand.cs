using System.Net;
using ForwardingClient;
using ForwardingClientExample.CommandSystem;

namespace ForwardingClientExample.Commands;

public class ConnectCommand : ITextCommand
{
    public string CommandName => "connect";
    public string Syntax => "/connect *IpAddress* *Port*";
    
    public string? Execute(string commandString)
    {
        var arguments = CommandExtractor.GetArguments(commandString);

        if (arguments.Count != 2)
        {
            return "Invalid arguments. Syntax: " + Syntax;
        }
        
        IPAddress ipAddress;
        int port;
        
        try
        {
            ipAddress = IPAddress.Parse(arguments[0]);
            port = Int32.Parse(arguments[1]);
        }
        catch (FormatException)
        {
            return "Invalid arguments. Syntax: " + Syntax;
        }

        try
        {
            var result = client.ConnectAsync(ipAddress, port).GetAwaiter().GetResult();
            if (result)
                return $"Successfully connected to: {ipAddress}:{port}";

            return $"Could not connected to: {ipAddress}:{port}";
        }
        catch (Exception e)
        {
            return e.Message;
        }
    }

    private FwClient client;

    public ConnectCommand(FwClient fwClient)
    {
        client = fwClient;
    }
}