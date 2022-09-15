using System.Net;
using Unnamed_Networking_Plugin;

namespace Unnamed_Networking_Plugin_Tests;

public class Tests
{
    [SetUp]
    public void Setup()
    {
        new Thread(TestServer).Start();
    }

    private void TestServer()
    {
        var networkPlugin = new UnnamedNetworkPluginClient(25565);
        
        // Send the same received package back to all connected clients.
        networkPlugin.PackageReceived += networkPlugin.SendPackageToAllConnections;
    }

    private UnnamedNetworkPluginClient GetConnectedClient()
    {
        var networkPlugin = new UnnamedNetworkPluginClient(25565);
        networkPlugin.AddConnection(IPAddress.Parse("127.1.1.1"));
        return networkPlugin;
    }

    [Test]
    public void ConnectionCanBeEstablished()
    {
        Assert.Pass();
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