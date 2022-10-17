using System.Net;
using NUnit.Framework.Internal.Execution;
using Unnamed_Networking_Plugin;
using Unnamed_Networking_Plugin.Interfaces;

namespace Unnamed_Networking_Plugin_Tests;

public class UnnamedNetworkingPluginTests
{
    private const int TimeOutLimit = 2000;

    /// <summary>
    /// Gets a networking client with the provided port.
    /// </summary>
    /// <param name="port">Port to listen to</param>
    /// <returns>Network client configured for selected port</returns>
    private static UnnamedNetworkPluginClient GetClient(int port = 25565)
    {
        var networkPlugin = new UnnamedNetworkPluginClient(port, new Logger(), new JsonSerializerAdapter());
        return networkPlugin;
    }

    /// <summary>
    /// Timeout task. Completes after the constant delay has passed.
    /// </summary>
    private static async Task Timeout()
    {
        Console.WriteLine("Started timer");
        await Task.Delay(TimeOutLimit);
        Console.WriteLine("Time has run out");
    }

    /// <summary>
    /// Listener task. Completes once the provided SemaphoreSlim gets released.
    /// </summary>
    /// <param name="signal">Signal to listen for.</param>
    private static async Task Listener(SemaphoreSlim signal)
    {
        Console.WriteLine("Started listener");
        await signal.WaitAsync();
        Console.WriteLine("Listener triggered");
    }

    /// <summary>
    /// Listener task. Completes once all of the provided SemaphoreSlim objects are released.
    /// </summary>
    /// <param name="signals">Signal to listen for.</param>
    private static async Task Listener(IEnumerable<SemaphoreSlim> signals)
    {
        Console.WriteLine("Started listener");
        foreach (var signal in signals)
        {
            await signal.WaitAsync();
        }

        Console.WriteLine("Listener triggered");
    }

    /// <summary>
    /// Checks if the provided IPackage is a TestPackage, then releases a semaphoreSlim once complete.
    /// </summary>
    /// <param name="receivedPackage">Received IPackage to compare with baseline package.</param>
    /// <param name="baselinePackage"></param>
    /// <param name="signal">Signal to release once complete.</param>
    /// <returns>The received package if type match, otherwise null.</returns>
    private static TestPackage? CheckPackage(IPackage receivedPackage, TestPackage baselinePackage,
        SemaphoreSlim signal)
    {
        Console.WriteLine("Package received");
        if (receivedPackage.Type == baselinePackage.Type)
        {
            Console.WriteLine("Package type match");
            var resultPackage = receivedPackage as TestPackage;
            signal.Release();
            return resultPackage;
        }

        Console.WriteLine("Package received with problems");
        signal.Release();
        return null;
    }


    [SetUp]
    public void Prepare()
    {
        mainClient = GetClient(25565);
        receiveClient25566 = GetClient(25566);
        receiveClient25567 = GetClient(25567);
        receiveClient25568 = GetClient(25568);
        receiveClient25569 = GetClient(25569);
    }

    [TearDown]
    public void Cleanup()
    {
        var stopTask1 = mainClient.StopListener();
        var stopTask2 = receiveClient25566.StopListener();
        var stopTask3 = receiveClient25567.StopListener();
        var stopTask4 = receiveClient25568.StopListener();
        var stopTask5 = receiveClient25569.StopListener();
        Task.WaitAll(stopTask1, stopTask2, stopTask3, stopTask4, stopTask5);
        // await Timeout();
    }

    private UnnamedNetworkPluginClient mainClient;
    private UnnamedNetworkPluginClient receiveClient25566;
    private UnnamedNetworkPluginClient receiveClient25567;
    private UnnamedNetworkPluginClient receiveClient25568;
    private UnnamedNetworkPluginClient receiveClient25569;
    

    // #################################################################################################################
    // ## Tests ########################################################################################################
    // #################################################################################################################
    
    [Test]
    public async Task ClientReportsSuccessfulOutgoingConnection()
    {
        Assert.IsTrue(await mainClient.AddConnection(IPAddress.Loopback, 25566));
    }
    
    [Test]
    public async Task ClientReportsUnsuccessfulOutgoingConnection()
    {
        Assert.IsFalse(await mainClient.AddConnection(IPAddress.Loopback, 0));
    }

    [Test]
    public void ClientReportsSuccessfulIncomingConnection()
    {
        // Listener
        SemaphoreSlim signal = new SemaphoreSlim(0, 1);
        receiveClient25566.ConnectionSuccessful += (sender, args) =>
        {
            Console.WriteLine("Received connection event triggered");
            signal.Release();
        };
    
        var timeout = Timeout();
        var listener = Listener(signal);
    
        // Create connection
        mainClient.AddConnection(IPAddress.Loopback, 25566);
    
        // Await and test result.
        Task.WaitAny(timeout, listener);
        Assert.IsTrue(listener.IsCompleted);
    }

    [Test]
    public async Task PackageCanBeTransmitted()
    {
        TestPackage package = new TestPackage(new TestData("Test Package", 42, 3.13f));

        TestPackage? resultPackage = null;
        
        SemaphoreSlim signal = new SemaphoreSlim(0, 1);
        receiveClient25566.PackageReceived += (o, args) =>
        { 
            resultPackage = CheckPackage(args.ReceivedPackage, package, signal);
        };

        var timeout = Timeout();
        var listener = Listener(signal);

        await mainClient.AddConnection(IPAddress.Loopback, 25566);
        await mainClient.SendPackageToAllConnections(package);

        Task.WaitAny(timeout, listener);
        
        Assert.That(resultPackage, !Is.EqualTo(null));
        Assert.That(resultPackage.TestData, Is.EqualTo(package.TestData));
    }
    
    [Test]
    public void ClientCanConnectToMultipleClients()
    {
        // Listeners:
        var signal1 = new SemaphoreSlim(0, 1);
        var signal2 = new SemaphoreSlim(0, 1);
        receiveClient25566.ConnectionSuccessful += (_, _) =>
        {
            Console.WriteLine("Client1 connection event triggered");
            signal1.Release();
        };
        receiveClient25567.ConnectionSuccessful += (_, _) =>
        {
            Console.WriteLine("Client2 connection event triggered");
            signal2.Release();
        };

        var timeout = Timeout();
        var listener = Listener(new[] {signal1, signal2});

        // Create connections
        mainClient.AddConnection(IPAddress.Loopback, 25566);
        mainClient.AddConnection(IPAddress.Loopback, 25567);

        Task.WaitAny(timeout, listener);
        Assert.IsTrue(listener.IsCompleted);
    }
}