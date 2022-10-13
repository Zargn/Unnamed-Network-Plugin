using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;
using Unnamed_Networking_Plugin.Interfaces;

namespace Unnamed_Networking_Plugin;

public class Listener
{
    private UnnamedNetworkPluginClient unnamedNetworkPluginClient;
    private bool active = false;
    private int listenPort;
    
    private ILogger logger;
    
    
    public Listener(UnnamedNetworkPluginClient unnamedNetworkPluginClient, int port, ILogger logger)
    {
        this.unnamedNetworkPluginClient = unnamedNetworkPluginClient;
        listenPort = port;
        this.logger = logger;
    }

    public void Start()
    {
        if (active)
            throw new Exception(
                "Start was called on a listener that was already running. Please check your code for duplicate calls.");
        active = true;

        listenForIncomingConnectionsTask = ListenForIncomingConnections();
    }

    private Task listenForIncomingConnectionsTask;

    public void Destroy()
    {
        listenForIncomingConnectionsTask.Dispose();
    }

    private async Task ListenForIncomingConnections()
    {
        logger.Log(this, $"Started listening on port: {listenPort}", LogType.Information);
        TcpListener tcpListener = new TcpListener(IPAddress.Any, listenPort);
        tcpListener.Start();
        
        while (true)
        {
            var client = await tcpListener.AcceptTcpClientAsync();
            
            logger.Log(this, $"Connection request received.", LogType.Information);

            var connection = new Connection(client, client.GetStream(), unnamedNetworkPluginClient.jsonSerializer, unnamedNetworkPluginClient.logger);
            unnamedNetworkPluginClient.AddConnectionToList(connection);
        }
    }
}