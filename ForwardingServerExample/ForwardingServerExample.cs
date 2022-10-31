

using ForwardingServer;

namespace ForwardingServerExample;

public class ForwardingServerExample
{
    public static void Main()
    {
        var server = new ForwardingServerExample();
        server.StartServer();
    }

    private void StartServer()
    {
        var identification = new UserIdentification("ForwardingServer");
        var jsonSerializer = new JsonSerializerAdapter();
        var logFileController = new LogFileController();
        var forwardingServer = new FwServer(25564, logFileController, jsonSerializer, new UserIdentificationPackage(identification));

        var serverTask = forwardingServer.Run();
        
        // TODO: Ask user for cancellation.
    }
}