using System.Net;
using System.Net.Sockets;

namespace Unnamed_Networking_Plugin;

public class Listener
{
    private UnnamedNetworkPluginClient unnamedNetworkPluginClient;
    private bool active = false;
    private int listenPort;
    
    
    public Listener(UnnamedNetworkPluginClient unnamedNetworkPluginClient, int port)
    {
        this.unnamedNetworkPluginClient = unnamedNetworkPluginClient;
        listenPort = port;
    }

    public void Start()
    {
        if (active)
            throw new Exception(
                "Start was called on a listener that was already running. Please check your code for duplicate calls.");
        active = true;

        var listenForIncomingConnectionsTask = ListenForIncomingConnections();
    }

    private async Task ListenForIncomingConnections()
    {
        TcpListener tcpListener = new TcpListener(IPAddress.Any, listenPort);
        tcpListener.Start();
        
        while (true)
        {
            var client = await tcpListener.AcceptTcpClientAsync();
            var connection = new Connection(client, client.GetStream(), unnamedNetworkPluginClient.jsonSerializer, unnamedNetworkPluginClient.logger);
            unnamedNetworkPluginClient.AddConnectionToList(connection);
        }
    }
}