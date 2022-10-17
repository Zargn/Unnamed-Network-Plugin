using System.Net;
using NUnit.Framework.Internal.Execution;
using Unnamed_Networking_Plugin;
using Unnamed_Networking_Plugin.Interfaces;

namespace Unnamed_Networking_Plugin_Tests;

public class UnnamedNetworkingPluginTests
{
    private UnnamedNetworkPluginClient localServer;
    private EventWaitHandle waitHandle = new(false, EventResetMode.ManualReset);

    private const int TimeOutLimit = 2000;
    
    [SetUp]
    public void Setup()
    {
        // new Thread(TestServer).Start();
    }
    
    private UnnamedNetworkPluginClient GetClient(int port = 25565)
    {
        var networkPlugin = new UnnamedNetworkPluginClient(port, new Logger(), new JsonSerializerAdapter());
        return networkPlugin;
    }

    private void TestServer()
    {
        localServer = new UnnamedNetworkPluginClient(25565, new Logger(), new JsonSerializerAdapter());
        
        // Send the same received package back to all connected clients.
        localServer.PackageReceived += HandlePackageReceivedEvent;
    }

    private void HandlePackageReceivedEvent(object? sender, PackageReceivedEventArgs e)
    {
        localServer.SendPackageToAllConnections(e.ReceivedPackage);
    }

    private async Task Timeout()
    {
        Console.WriteLine("Started timer");
        await Task.Delay(TimeOutLimit);
        Console.WriteLine("Time has run out");
    }

    private async Task Listener(SemaphoreSlim signal)
    {
        Console.WriteLine("Started listener");
        await signal.WaitAsync();
        Console.WriteLine("Listener triggered");
    }
    
    private async Task Listener(IEnumerable<SemaphoreSlim> signals)
    {
        Console.WriteLine("Started listener");
        foreach (var signal in signals)
        {
            await signal.WaitAsync();
        }
        Console.WriteLine("Listener triggered");
    }

    private TestPackage? CheckPackage(IPackage receivedPackage, TestPackage baselinePackage, SemaphoreSlim signal)
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


    // TODO: HAve server and client instead be on the same thread and created in each test, using async instead of threads.
    
    // #################################################################################################################
    // ## Tests                                                                                                       ##
    // #################################################################################################################
    
    [Test]
    public async Task ClientReportsSuccessfulOutgoingConnection()
    {
        var client = GetClient();
        Console.WriteLine("Got client");
        var connectionResult = client.AddConnection(IPAddress.Loopback, 25565);
        await connectionResult;
        Assert.IsTrue(connectionResult.Result);
    }
    
    [Test]
    public async Task ClientReportsUnsuccessfulOutgoingConnection()
    {
        var client = GetClient(25566);
        Console.WriteLine("Got client");
        var connectionResult = client.AddConnection(IPAddress.Loopback, 0);
        await connectionResult;
        Assert.IsFalse(connectionResult.Result);
    }

    [Test]
    public async Task ClientReportsSuccessfulIncomingConnection()
    {
        // Create instance
        var client = GetClient(25567);
        // var server = GetClient();
        
        // Listener
        SemaphoreSlim signal = new SemaphoreSlim(0, 1);
        client.ConnectionSuccessful += (sender, args) =>
        {
            Console.WriteLine("Received connection event triggered");
            signal.Release();
        };
    
        var timeout = Timeout();
        var listener = Listener(signal);
    
        // Create connection
        client.AddConnection(IPAddress.Loopback, 25567);
    
        // Await and test result.
        Task.WaitAny(timeout, listener);
        Assert.IsTrue(listener.IsCompleted);
    }

    [Test]
    public async Task ClientCanConnectToMultipleClients()
    {
        var client = GetClient(25569);
        var client1 = GetClient(25570);
        var client2 = GetClient(25571);
        
        // Listeners:
        var signal1 = new SemaphoreSlim(0, 1);
        var signal2 = new SemaphoreSlim(0, 1);
        client1.ConnectionSuccessful += (sender, args) =>
        {
            Console.WriteLine("Client1 connection event triggered");
            signal1.Release();
        };
        client2.ConnectionSuccessful += (sender, args) =>
        {
            Console.WriteLine("Client2 connection event triggered");
            signal2.Release();
        };

        var timeout = Timeout();
        var listener = Listener(new[] {signal1, signal2});

        // Create connections
        client.AddConnection(IPAddress.Loopback, 25570);
        client.AddConnection(IPAddress.Loopback, 25571);

        Task.WaitAny(timeout, listener);
        Assert.IsTrue(listener.IsCompleted);
    }


    // [Test]
    // public void ConnectionToInvalidIpReturnsError()
    // {
    //     
    // }

    [Test]
    public async Task PackageCanBeTransmitted()
    {
        var sender = GetClient(25568);

        TestPackage package = new TestPackage(new TestData("Test Package", 42, 3.13f));

        TestPackage? resultPackage = null;
        
        SemaphoreSlim signal = new SemaphoreSlim(0, 1);
        sender.PackageReceived += (o, args) =>
        { 
            resultPackage = CheckPackage(args.ReceivedPackage, package, signal);
        };

        var timeout = Timeout();
        var listener = Listener(signal);

        await sender.AddConnection(IPAddress.Loopback, 25568);
        await sender.SendPackageToAllConnections(package);

        Task.WaitAny(timeout, listener);
        
        Assert.That(resultPackage, !Is.EqualTo(null));
        Assert.That(resultPackage.TestData, Is.EqualTo(package.TestData));
    }
    
    // [Test]
    // public void MessageIsSentCorrectly()
    // {
    //     
    // }
    //
    // [Test]
    // public void MessageIsReceivedCorrectly()
    // {
    //
    // }
}