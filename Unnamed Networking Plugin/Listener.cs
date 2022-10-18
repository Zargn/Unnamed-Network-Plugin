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

    private Task listenTask;

    private CancellationTokenSource cancelTokenSource;

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

        cancelTokenSource = new CancellationTokenSource(); 
        listenTask = ListenForIncomingConnections();
    }

    public async Task Stop()
    {
        if (!active)
            return;
        
        cancelTokenSource.Cancel();
        active = false;
        await listenTask;
    }

    
    private async Task ListenForIncomingConnections()
    {
        var tcpListener = new TcpListener(IPAddress.Any, listenPort);
        
        var token = cancelTokenSource.Token;
        logger.Log(this, $"Started listening on port: {listenPort}", LogType.Information);
        tcpListener.Start();
        
        try
        {
            while (true)
            {
                var client = await tcpListener.AcceptTcpClientAsync(token);

                logger.Log(this, $"Connection request received.", LogType.Information);

                var connection = new Connection(client, client.GetStream(), unnamedNetworkPluginClient.jsonSerializer, unnamedNetworkPluginClient.logger);
                unnamedNetworkPluginClient.AddConnectionToList(connection);
            }
        }
        catch (OperationCanceledException)
        {
            tcpListener.Stop();
        }
    }
}