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
    
    private UnnamedNetworkPluginClient GetConnectedClient()
    {
        var networkPlugin = new UnnamedNetworkPluginClient(25565);
        networkPlugin.AddConnection(IPAddress.Parse("127.1.1.1"));
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

    private bool ConnectionSuccessful(UnnamedNetworkPluginClient client)
    {
        client.ConnectionSuccessful += HandleSuccessfulConnectionEvent;
        var result = waitHandle.WaitOne(1000);
        client.ConnectionSuccessful -= HandleSuccessfulConnectionEvent;
        return result;
    }
    
    

    [Test]
    public void ClientReportsSuccessfulConnection()
    {
        var client = GetConnectedClient();
        Assert.IsTrue(ConnectionSuccessful(client));
    }
    
    [Test]
    public void ServerReportsSuccessfulConnection()
    {
        var client = GetConnectedClient();
        Assert.IsTrue(ConnectionSuccessful(localServer));
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