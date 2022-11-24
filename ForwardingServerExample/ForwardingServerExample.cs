using System.Data;
using ForwardingServer;
using ForwardingServerExampleShared;
using Unnamed_Networking_Plugin.Interfaces;
using Unnamed_Networking_Plugin.Resources;

namespace ForwardingServerExample;

public class ForwardingServerExample
{
    public static async Task Main()
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
        
        // Temp();

        var server = new ForwardingServerExample();
        await server.StartServer();
        // task.Wait();
        Console.WriteLine("Press enter to close window.");
        Console.ReadLine();
    }
    
    // private static async void Temp()
    // {
    //     await Task.Delay(1000);
    //     var n = Int32.Parse("nr21");
    // }
    
    private async Task StartServer()
    {
        var identification = new UserIdentification("ForwardingServer");
        var jsonSerializer = new JsonSerializerAdapter();
        var logFileController = new LogFileController();
        var forwardingServer = new FwServer<UserIdentification>(25564, logFileController, jsonSerializer, new UserIdentificationPackage(identification));

        var serverTask = forwardingServer.Run();
        
        // TODO: Ask user for cancellation.
        while (true)
        {
            Console.WriteLine("Write Q to quit.");
            var result = Console.ReadLine();
            if (result.ToLower() != "q") continue;
            Console.WriteLine("Quitting...");
            await forwardingServer.Stop();
            return;
        }
    }
}

