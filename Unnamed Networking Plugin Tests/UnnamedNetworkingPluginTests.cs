using System.Net;
using NUnit.Framework.Internal.Execution;
using Unnamed_Networking_Plugin;

namespace Unnamed_Networking_Plugin_Tests;

public class UnnamedNetworkingPluginTests
{
    private UnnamedNetworkPluginClient localServer;
    private EventWaitHandle waitHandle = new(false, EventResetMode.ManualReset);

    [SetUp]
    public void Setup()
    {
        new Thread(TestServer).Start();
    }
    
    private UnnamedNetworkPluginClient GetClient()
    {
        var networkPlugin = new UnnamedNetworkPluginClient(25565);
        return networkPlugin;
    }

    private void TestServer()
    {
        localServer = new UnnamedNetworkPluginClient(25565);
        
        // Send the same received package back to all connected clients.
        localServer.PackageReceived += HandlePackageReceivedEvent;
    }

    private void HandlePackageReceivedEvent(object? sender, PackageReceivedEventArgs e)
    {
        localServer.SendPackageToAllConnections(e.ReceivedPackage);
    }
    
    private void HandleSuccessfulConnectionEvent(object? sender, ConnectionReceivedEventArgs e)
    {
        SetWaitHandle();
    }

    private void SetWaitHandle()
    {
        waitHandle.Set();
    }

    //TODO: THIS DOES NOT WORK.
    //Execution is running synchronous instead of async.
    private async Task<bool> ConnectionSuccessful(UnnamedNetworkPluginClient client)
    {
        Console.WriteLine("Start scan");
        client.ConnectionSuccessful += HandleSuccessfulConnectionEvent;
        var tempTask = temp();
        var result = await tempTask;
        client.ConnectionSuccessful -= HandleSuccessfulConnectionEvent;
        Console.WriteLine($"End scan [{result}]");
        
        return result;
    }

    // This is where the problem lies. WaitOne() does not work async, and instead just pauses the entire thread.
    private Task<bool> temp()
    {
        return new Task<bool>(() =>
        {
            // var result = waitHandle.WaitOne(1000);
            Thread.Sleep(1000);

            // return result;
            return false;
        });
    }


    
    // #################################################################################################################
    // ## Tests                                                                                                       ##
    // #################################################################################################################
    
    [Test]
    public async Task ClientReportsSuccessfulConnection()
    {
        var client = GetClient();
        Console.WriteLine("Got client");
        var connectionResult = client.AddConnection(IPAddress.Loopback);
        await connectionResult;
        Assert.IsTrue(connectionResult.Result);
    }
    
    [Test]
    public async Task ClientReportsUnsuccessfulConnection()
    {
        var client = GetClient();
        Console.WriteLine("Got client");
        var connectionResult = client.AddConnection(IPAddress.Loopback);
        await connectionResult;
        Assert.IsFalse(connectionResult.Result);
    }

    [Test]
    public void ServerReportsSuccessfulConnection()
    {
        var client = GetClient();
        var connectionSuccessful = ConnectionSuccessful(localServer);
        client.AddConnection(IPAddress.Loopback);
        Assert.IsTrue(connectionSuccessful.Result);
    }

    [Test]
    public void ConnectionToInvalidIpReturnsError()
    {
        
    }

    [Test]
    public void MessageIsSentCorrectly()
    {
        
    }

    [Test]
    public void MessageIsReceivedCorrectly()
    {
        
    }
    
}