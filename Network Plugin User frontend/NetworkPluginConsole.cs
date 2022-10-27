using System.Net;
using ConsoleUtilities;
using Unnamed_Networking_Plugin;

namespace Network_Plugin_User_frontend;

public static class Initializer
{
    public static void Main()
    {
        string? name = SimpleConsoleHelpers.RequestStringInput("Please enter your name: ");
        NetworkPluginConsole networkPluginConsole = new(name);
        networkPluginConsole.Run();
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

    public void Run()
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
        var mainLoop = MainLoop();
        Task.WaitAny(mainLoop);
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
        Quit
    }

    private Operation GetUserInput()
    {
        while (true)
        {
            Console.WriteLine("[Q] = quit, [S] = send message, [SA] = send message to all, [C] = add connection to client, [D] = disconnect from client.");
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
                default:
                    Console.WriteLine("Invalid input. Try again!");
                    break;
            }
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
        var ipString =SimpleConsoleHelpers.RequestStringInput("Please enter target ip:");
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

        var port = SimpleConsoleHelpers.RequestIntInput("Please enter port:");
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
            SimpleConsoleHelpers.RequestStringInput("Please enter name of client to disconnect.");
        client.RemoveConnection(new NameIdentifier(targetConnectionName));
    }
}