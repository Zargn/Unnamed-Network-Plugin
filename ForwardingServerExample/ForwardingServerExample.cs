using ForwardingServer;

namespace ForwardingServerExample;

public class ForwardingServerExample
{
    public static void Main()
    {
        var server = new ForwardingServerExample();
        var task = server.StartServer();
    }

    private async Task StartServer()
    {
        var identification = new UserIdentification("ForwardingServer");
        var jsonSerializer = new JsonSerializerAdapter();
        var logFileController = new LogFileController();
        var forwardingServer = new FwServer(25564, logFileController, jsonSerializer, new UserIdentificationPackage(identification));

        var serverTask = forwardingServer.Run();
        
        // TODO: Ask user for cancellation.
        while (true)
        {
            Console.WriteLine("Write Q to quit.");
            var result = Console.ReadLine();
            if (result.ToLower() != "q") continue;
            await forwardingServer.Stop();
            return;
        }
    }
}