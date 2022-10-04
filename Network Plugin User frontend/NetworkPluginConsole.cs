using System.Net;
using Unnamed_Networking_Plugin;

namespace Network_Plugin_User_frontend;

public static class Initializer
{
    public static void Main()
    {
        NetworkPluginConsole networkPluginConsole = new();
        networkPluginConsole.Run();
    }
}

public class NetworkPluginConsole
{
    private UnnamedNetworkPluginClient client = new(25565);
    private UnnamedNetworkPluginClient server = new(25566);
    
    public void Run()
    {
        new Thread(ServerThread).Start();
        MainLoop();
    }

    private void ServerThread()
    {
        
    }
    
    private async Task MainLoop()
    {
        await client.AddConnection(IPAddress.Loopback, 25565);
        while (true)
        {
            var operation = GetUserInput();
            switch (operation)
            {
                case Operation.Send:
                    SendMessage();
                    break;
                case Operation.Request:
                    RequestMessages();
                    break;
                case Operation.Quit:
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private enum Operation
    {
        Send,
        Request,
        Quit
    }

    private Operation GetUserInput()
    {
        while (true)
        {
            Console.WriteLine("[Q] = quit, [S] = send message, [R] = request list of sent messages");
            var inputString = Console.ReadLine();
            switch (inputString.ToLower())
            {
                case "q":
                    return Operation.Quit;
                case "s":
                    return Operation.Send;
                case "r":
                    return Operation.Request;
                default:
                    Console.WriteLine("Invalid input. Try again!");
                    break;
            }
        }
    }

    private void SendMessage()
    {
        Console.WriteLine("Please enter message to send: ");
        var message = Console.ReadLine();
        throw new NotImplementedException();
    }

    private void RequestMessages()
    {
        throw new NotImplementedException("Requesting messages from the server has not been implemented yet!");
    }
}