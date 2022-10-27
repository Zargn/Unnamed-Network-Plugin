using System.Net;
using System.Net.Sockets;
using System.Runtime.Loader;
using System.Threading.Channels;
using Unnamed_Networking_Plugin.Interfaces;
using Unnamed_Networking_Plugin.Resources;

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



                
                // Todo: Might be better to just reset the signal instead of creating a new instance.
                temporarySignal = new SemaphoreSlim(0, 1);
                temporaryRemoteIdentificationPackage = null;

                connection.PackageReceived += GatherIdentificationPackage;

                var timeout = Timeout();
                var signalListener = SignalListener(temporarySignal);
                
                // No need to await this...
                // Or maybe we want to await it at the end of this method to make sure the receiver also added us as a connection?
                await connection.SendPackage(unnamedNetworkPluginClient.identificationPackage);
                
                Task.WaitAny(new Task[] {timeout, signalListener}, token);

                connection.PackageReceived -= GatherIdentificationPackage;
                
                if (signalListener.IsCompleted)
                {
                    if (temporaryRemoteIdentificationPackage == null)
                    {
                        logger.Log(this, "Received identification package was invalid. Disconnecting...", LogType.HandledError);
                        connection.Disconnect();
                        continue;
                    }

                    var remoteInformation = temporaryRemoteIdentificationPackage.ExtractConnectionInformation();

                    if (remoteInformation == null)
                    {
                        logger.Log(this, "Received information was null. Please check your identification package class.", LogType.HandledError);
                        connection.Disconnect();
                        continue;
                    }

                    if (unnamedNetworkPluginClient.CheckIfIdentificationAlreadyPresent(remoteInformation))
                    {
                        logger.Log(this, $"Connecting client from {remoteInformation} is already connected. Disconnecting...", LogType.Information);
                        // TODO: Send package informing client about the denied connection before disconnecting 
                        connection.Disconnect();
                        continue;
                    }

                    logger.Log(this, $"Received connection from {remoteInformation}", LogType.Information);
                    connection.ConnectionInformation = remoteInformation;
                    unnamedNetworkPluginClient.AddConnectionToList(connection);
                    continue;
                }
        
                connection.Disconnect();
                logger.Log(this, "Remote did not provide identification. Disconnecting...", LogType.HandledError);
            }
        }
        catch (OperationCanceledException)
        {
            logger.Log(this, "Listener stopped.", LogType.Information);
            tcpListener.Stop();
        }
    }

    private SemaphoreSlim temporarySignal;
    private IdentificationPackage? temporaryRemoteIdentificationPackage;
    
    private void GatherIdentificationPackage(object? sender, PackageReceivedEventArgs args)
    {
        temporaryRemoteIdentificationPackage = args.ReceivedPackage as IdentificationPackage;
        temporarySignal.Release();
    }
    
    /// <summary>
    /// Timeout task. Completes after the constant delay has passed.
    /// </summary>
    private static async Task Timeout()
    {
        await Task.Delay(1000);
    }

    /// <summary>
    /// Listener task. Completes once the provided SemaphoreSlim gets released.
    /// </summary>
    /// <param name="signal">Signal to listen for.</param>
    private static async Task SignalListener(SemaphoreSlim signal)
    {
        await signal.WaitAsync();
    }

    public override string ToString()
    {
        return $"{base.ToString()}:{unnamedNetworkPluginClient.identificationPackage.ExtractConnectionInformation()}";
    }
}