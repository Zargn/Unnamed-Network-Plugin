using ForwardingServer;
using ForwardingServer.Group.Resources;
using Unnamed_Networking_Plugin.Interfaces;
using Unnamed_Networking_Plugin.Resources;

namespace ForwardingServerExample;

public class ForwardingServerExample
{
    public static void Main()
    {
        // var jsonSerializer = new JsonSerializerAdapter();
        // var fwp = new ForwardingPackage("hiJson", new UserIdentification("Test"));
        // var json = jsonSerializer.Serialize(fwp);
        // var package = jsonSerializer.DeSerialize<Package>(json);
        // var objectType = AppDomain.CurrentDomain.GetAssemblies()
        //     .Select(assembly => assembly.GetType(package.Type))
        //     .SingleOrDefault(type => type != null);
        // var fwp2 = jsonSerializer.Deserialize(json, objectType);
        // var mfwp = fwp2 as ForwardingPackage;
        // var username = mfwp.TargetInformation.ToString();
        // Console.WriteLine(username);

        var server = new ForwardingServerExample();
        var task = server.StartServer();
        task.Wait();
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

