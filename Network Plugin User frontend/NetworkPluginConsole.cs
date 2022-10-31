using System.Net;
using ConsoleUtilities;
using Unnamed_Networking_Plugin;

namespace Network_Plugin_User_frontend;

public static class Initializer
{
    public static async Task Main()
    {
        string? name = SimpleConsoleHelpers.RequestStringInput("Please enter your name: ");
        NetworkPluginConsole networkPluginConsole = new(name);
        var client = networkPluginConsole.Run();
        await client;
    }
}

public class NetworkPluginConsole
{
    private UnnamedNetworkPluginClient client;
    // private UnnamedNetworkPluginClient server = new(25566, new Logger(), new JsonSerializerAdapter(), new PortIdentifier(25566));
    private NameIdentifier nameIdentifier;


    public NetworkPluginConsole(string name)
    {
        nameIdentifier = new NameIdentifier(name);
        client = new(25565, new Logger(), new JsonSerializerAdapter(), new NameIdentifierPackage(nameIdentifier));
    }

    public async Task Run()
    {
        client.ConnectionSuccessful += (sender, args) =>
        {
            Console.WriteLine($"User with name: [{args.ConnectionInformation}] has been connected!");
        };
        client.PackageReceived += (sender, args) =>
        {
            Console.WriteLine(args.ReceivedPackage);
        };
        client.ConnectionLost += (sender, args) =>
        {
            Console.WriteLine($"User with name: [{args.ConnectionInformation}] has disconnected.");
        };
        await MainLoop();
        Console.WriteLine("Loop ended.");
    }

    private async Task MainLoop()
    {
        while (true)
        {
            var operation = GetUserInput();
            switch (operation)
            {
                case Operation.Send:
                    await SendMessage();
                    break;
                case Operation.SendAll:
                    SendMessageToAll();
                    break;
                case Operation.Connect:
                    await Connect();
                    break;
                case Operation.Disconnect:
                    Disconnect();
                    break;
                case Operation.List:
                    ListConnections();
                    break;
                case Operation.Quit:
                    Console.WriteLine("Quit.");
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private enum Operation
    {
        Send,
        SendAll,
        Connect,
        Disconnect,
        Quit,
        List
    }

    private Operation GetUserInput()
    {
        while (true)
        {
            Console.WriteLine("[Q] = quit, [S] = send message, [SA] = send message to all, [C] = add connection to client, [D] = disconnect from client, [L] = list connections.");
            var inputString = Console.ReadLine();
            switch (inputString.ToLower())
            {
                case "q":
                    return Operation.Quit;
                case "s":
                    return Operation.Send;
                case "sa":
                    return Operation.SendAll;
                case "c":
                    return Operation.Connect;
                case "d":
                    return Operation.Disconnect;
                case "l":
                    return Operation.List;
                default:
                    Console.WriteLine("Invalid input. Try again!");
                    break;
            }
        }
    }

    private async Task SendMessage()
    {
        var targetUser = SimpleConsoleHelpers.RequestStringInput("Please enter target username: ");
        try
        {
            var targetConnection = client.GetConnectionFromList(new NameIdentifier(targetUser));
            var message = SimpleConsoleHelpers.RequestStringInput("Please enter message to send: ");
            await targetConnection.SendPackage(new TextMessagePackage(message, nameIdentifier));
        }
        catch (KeyNotFoundException)
        {
            Console.WriteLine("Invalid username.");
        }
    }

    private void SendMessageToAll()
    {
        Console.WriteLine("Please enter message to send: ");
        var message = Console.ReadLine();
        var task = client.SendPackageToAllConnections(new TextMessagePackage(message, nameIdentifier));
    }

    private async Task Connect()
    {
        var ipString =SimpleConsoleHelpers.RequestStringInput("Please enter target ip: ");
        IPAddress ip = null;
        try
        {
            ip = IPAddress.Parse(ipString);
        }
        catch (Exception e)
        {
            Console.WriteLine("Provided ip was invalid.");
            return;
        }

        var port = SimpleConsoleHelpers.RequestIntInput("Please enter port: ");
        var connection = client.AddConnection(ip, port);
        await connection;
        if (!connection.Result)
        {
            Console.WriteLine("Connection was unsuccessful. Please try again.");
        }
    }

    private void Disconnect()
    {
        var targetConnectionName =
            SimpleConsoleHelpers.RequestStringInput("Please enter name of client to disconnect: ");
        client.RemoveConnection(new NameIdentifier(targetConnectionName));
    }

    private void ListConnections()
    {
        Console.WriteLine("Connected clients:");
        foreach (var pair in client.Connections)
        {
            Console.WriteLine(pair.Key);
        }
    }
}