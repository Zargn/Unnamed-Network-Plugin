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


    // [Test]
    // public void ConnectionToInvalidIpReturnsError()
    // {
    //     
    // }

    [Test]
    public async Task PackageCanBeTransmitted()
    {
        var sender = GetClient(25568);
        // var receiver = GetClient();

        TestPackage package = new TestPackage(new TestData("Test Package", 42, 3.13f));

        bool success = false;

        SemaphoreSlim signal = new SemaphoreSlim(0, 1);
        sender.PackageReceived += (o, args) =>
        {
            Console.WriteLine("Package received");
            var receivedPackage = args.ReceivedPackage;
            if (receivedPackage.Type == package.Type)
            {
                Console.WriteLine("Package type match");
                Console.WriteLine(receivedPackage);

                var resultPackage = receivedPackage as TestPackage;
                Console.WriteLine(package.TestData);
                Console.WriteLine(resultPackage.TestData);
                
                if (Equals(resultPackage.TestData, package.TestData))
                {
                    Console.WriteLine("Package contents match");
                    success = true;
                    signal.Release();
                    return;
                }
            }

            success = false;
            Console.WriteLine("Package received with problems");
            signal.Release();
        };

        var timeout = Timeout();
        var listener = Listener(signal);

        await sender.AddConnection(IPAddress.Loopback, 25568);
        // sender.SendPackage(package, IPAddress.Loopback);
        await sender.SendPackageToAllConnections(package);

        Task.WaitAny(timeout, listener);
        Assert.IsTrue(success);
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