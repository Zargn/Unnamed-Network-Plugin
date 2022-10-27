﻿using System.Net;
using System.Net.Sockets;
using Unnamed_Networking_Plugin.EventArgs;
using Unnamed_Networking_Plugin.Interfaces;
using Unnamed_Networking_Plugin.Resources;


namespace Unnamed_Networking_Plugin;

public class UnnamedNetworkPluginClient
{
    internal ILogger logger;
    internal IJsonSerializer jsonSerializer;

    private readonly int port;
    private int nextConnectionId = 0;
    private Listener listener;
    private Type connectionIdentificationType;
    private SemaphoreSlim temporarySignal;
    private IdentificationPackage? temporaryRemoteIdentificationPackage;
    private Dictionary<IConnectionInformation, Connection> connections = new();

    public Dictionary<IConnectionInformation, Connection> Connections => connections;

    /// <summary>
    /// The currently configured identification package for this instance.
    /// </summary>
    public IdentificationPackage identificationPackage { get; private set; }
    
    /// <summary>
    /// Invoked when a package has been received from any connected client.
    /// Contains the package, package type, and ConnectionInformation of who sent it.
    /// </summary>
    public event EventHandler<PackageReceivedEventDetailedArgs>? PackageReceived;

    /// <summary>
    /// Invoked on a successful connection.
    /// Contains the finished connection object and its associated ConnectionInformation.
    /// </summary>
    public event EventHandler<ConnectionReceivedEventArgs>? ConnectionSuccessful;

    /// <summary>
    /// Invoked when a connection has been lost.
    /// Contains the ConnectionInformation of the client which was lost.
    /// </summary>
    public event EventHandler<ClientDisconnectedEventDetailedArgs>? ConnectionLost; 

    /// <summary>
    /// Constructor and configurator of the client.
    /// </summary>
    /// <param name="port">integer representing the port to listen to.</param>
    /// <param name="logger">ILogger implementer to handle logging.</param>
    /// <param name="jsonSerializer">IJsonSerializer implementer to handle serialization.</param>
    /// <param name="identificationPackage">The identification package for this client.</param>
    public UnnamedNetworkPluginClient(int port, ILogger logger, IJsonSerializer jsonSerializer, IdentificationPackage identificationPackage)
    {
        this.port = port;
        this.logger = logger;
        this.jsonSerializer = jsonSerializer;
        this.identificationPackage = identificationPackage;
        
        connectionIdentificationType = identificationPackage.GetType();
        listener = new Listener(this, port, logger);
        listener.Start();
    }

    // TODO: Look into a way to do this automatically when the user has discarded this object.
    /// <summary>
    /// Manually stop the connection listener.
    /// </summary>
    public async Task StopListener()
    {
        await listener.Stop();
    }

    /// <summary>
    /// Attempt to connect to the provided endpoint. If successful a connection object will be configured with the information provided by the endpoint.
    /// </summary>
    /// <param name="ipAddress">Target ip address.</param>
    /// <param name="targetPort">Target port.</param>
    /// <returns>If successful or not.</returns>
    public async Task<bool> AddConnection(IPAddress ipAddress, int targetPort)
    {
        var tcpClient = new TcpClient();
        try
        {
            await tcpClient.ConnectAsync(ipAddress, targetPort);
        }
        catch (Exception e)
        {
            logger.Log(this, @$"An error occured when attempting to connect to {ipAddress}:{targetPort} 
{e}", LogType.HandledError);
            return false;
        }

        var connection = new Connection(tcpClient ,tcpClient.GetStream(), jsonSerializer, logger);

        temporarySignal = new SemaphoreSlim(0, 1);
        temporaryRemoteIdentificationPackage = null;

        connection.PackageReceived += GatherIdentificationPackage;
        
        var timeout = Timeout();
        var signalListener = SignalListener(temporarySignal);

       await connection.SendPackage(identificationPackage);

        Task.WaitAny(timeout, signalListener);
        
        connection.PackageReceived -= GatherIdentificationPackage;
        
        if (signalListener.IsCompleted)
        {
            if (temporaryRemoteIdentificationPackage == null)
            {
                logger.Log(this, "Received identification package was invalid. Disconnecting...", LogType.HandledError);
                connection.Disconnect();
                return false;
            }

            var remoteInformation = temporaryRemoteIdentificationPackage.ExtractConnectionInformation();

            if (remoteInformation == null)
            {
                logger.Log(this, "Received information was null. Please check your identification package class.", LogType.HandledError);
                connection.Disconnect();
                return false;
            }
            
            if (CheckIfIdentificationAlreadyPresent(remoteInformation))
            {
                logger.Log(this, $"Connecting client from {remoteInformation} is already connected. Disconnecting...", LogType.Information);
                // TODO: Send package informing client about the denied connection before disconnecting 
                connection.Disconnect();
                return false;
            }

            logger.Log(this, $"Received connection from {remoteInformation}", LogType.Information);
            connection.ConnectionInformation = remoteInformation;
            AddConnectionToList(connection);
            return true;
        }
        
        logger.Log(this, "Remote did not provide identification. Disconnecting...", LogType.HandledError);
        connection.Disconnect();
        return false;
    }

    /// <summary>
    /// Handles listening to PackageReceived events from connections being set up.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
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

    /// <summary>
    /// Checks the Connections dictionary for connectionInformation matching the provided.
    /// </summary>
    /// <param name="connectionInformation">ConnectionInformation to search for.</param>
    /// <returns>If a match was found or not.</returns>
    internal bool CheckIfIdentificationAlreadyPresent(IConnectionInformation connectionInformation)
    {
        return Connections.ContainsKey(connectionInformation);
    }

    /// <summary>
    /// Configures events and adds the provided connection to the dictionary.
    /// </summary>
    /// <param name="connection"></param>
    internal void AddConnectionToList(Connection connection)
    {
        Connections.Add(connection.ConnectionInformation, connection);
        connection.PackageReceived += (o, args) =>
        {
            PackageReceived?.Invoke(o, new PackageReceivedEventDetailedArgs(args.ReceivedPackage, args.PackageType, connection.ConnectionInformation));
        };
        connection.ClientDisconnected += (o, args) =>
        {
            Connections.Remove(connection.ConnectionInformation);
            ConnectionLost?.Invoke(o, new ClientDisconnectedEventDetailedArgs(args.RemoteDisconnected, connection.ConnectionInformation));
        };
        ConnectionSuccessful?.Invoke(this, new ConnectionReceivedEventArgs(connection.ConnectionInformation, connection));
    }
    
    /// <summary>
    /// Tries to get a connection based on the provided ConnectionInformation.
    /// </summary>
    /// <param name="targetConnectionInformation"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException">If there is no connection with matching information.</exception>
    public Connection GetConnectionFromList(IConnectionInformation targetConnectionInformation)
    {
        return Connections[targetConnectionInformation];
    }

    /// <summary>
    /// Tries to disconnect a connection with matching ConnectionInformation.
    /// </summary>
    /// <param name="targetConnectionInformation"></param>
    /// <exception cref="KeyNotFoundException">If there is no connection with matching information.</exception>
    public void RemoveConnection(IConnectionInformation targetConnectionInformation)
    {
        Connections[targetConnectionInformation].Disconnect();
    }

    /// <summary>
    /// Tries to send the provided package to the client with provided ConnectionInformation.
    /// </summary>
    /// <param name="package">Package to be transmitted.</param>
    /// <param name="targetConnectionInformation">Target connection information.</param>
    /// <typeparam name="T">Package type</typeparam>
    /// <exception cref="KeyNotFoundException">If there is no connection with matching information.</exception>
    public async Task SendPackage<T>(T package, IConnectionInformation targetConnectionInformation)
        where T : IPackage
    {
        await Connections[targetConnectionInformation].SendPackage(package);
    }

    /// <summary>
    /// Sends the provided package to all connected clients. This will run even if there are no clients connected.
    /// </summary>
    /// <param name="package">Package to be transmitted.</param>
    /// <typeparam name="T">Package type.</typeparam>
    public async Task SendPackageToAllConnections<T>(T package)
    where T : IPackage
    {
        var sendTasks = Connections.Select(connectionEntry => connectionEntry.Value.SendPackage(package)).ToArray();
        await Task.WhenAll(sendTasks);
    }
}