using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using Unnamed_Networking_Plugin.Interfaces;
using Unnamed_Networking_Plugin.Resources;

namespace Unnamed_Networking_Plugin;

public class Connection
{
    public TcpClient TcpClient { get; }
    public Stream DataStream { get; private set; }
    
    public event EventHandler<PackageReceivedEventArgs>? PackageReceived;
    public event EventHandler<ClientDisconnectedEventArgs>? ClientDisconnected;
    
    
    // TODO: This property should probably be moved out of this class since it is only useful if you use the full app.
    public IConnectionInformation ConnectionInformation { get; set; }

    
    private StreamReader streamReader { get; set; }
    private StreamWriter streamWriter { get; set; }

    private IJsonSerializer jsonSerializer;
    private ILogger logger;

    private Task packageListenerTask;

    private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
    
    public bool Connected { get; private set; }


    public Connection(IJsonSerializer jsonSerializer, ILogger logger)
    {
        TcpClient = new TcpClient();
        this.jsonSerializer = jsonSerializer;
        this.logger = logger;
    }

    public Connection(TcpClient tcpClient, IJsonSerializer jsonSerializer, ILogger logger)
    {
        TcpClient = tcpClient;
        this.jsonSerializer = jsonSerializer;
        this.logger = logger;
        
        DataStream = TcpClient.GetStream();
        streamReader = new StreamReader(DataStream);
        streamWriter = new StreamWriter(DataStream);
        
        logger.Log(this, "Connection established", LogType.Information);
        
        packageListenerTask = PackageListener();

        Connected = true;
    }
    
    public async Task ConnectAsync(IPAddress ipAddress, int targetPort)
    {
        if (Connected)
        {
            logger.Log(this, $"ConnectAsync was called when the connection is already active. This does not work and should be stopped.", LogType.Error);
            throw new Exception(
                "This connection object is already actively connected and therefor can not connect again.");
        }
        
        try
        {
            await TcpClient.ConnectAsync(ipAddress, targetPort);
        }
        catch (Exception e)
        {
            logger.Log(this, @$"An error occured when attempting to connect to {ipAddress}:{targetPort} 
{e}", LogType.HandledError);
            throw;
        }

        // TODO: Move identification sending to here instead?
        
        
        
        
        
        
        DataStream = TcpClient.GetStream();
        streamReader = new StreamReader(DataStream);
        streamWriter = new StreamWriter(DataStream);
        
        logger.Log(this, "Connection established", LogType.Information);
        
        packageListenerTask = PackageListener();

        Connected = true;
    }

    public void Disconnect()
    {
        // cancellationTokenSource.Cancel();
        // streamReader.Close();
        TcpClient.Close();
        ClientDisconnected?.Invoke(this, new ClientDisconnectedEventArgs(false));

        // throw new NotImplementedException();
        // TcpClient.Close();
        // streamWriter.Close();
    }

    public async Task SendPackage<T>(T package)
    where T : IPackage
    {
        var json = jsonSerializer.Serialize(package, package.GetType());
        
        logger.Log(this, $"Sent json: {json}", LogType.Information);
        
        if (json == null)
        {
            logger.Log(this, "Result Json was null", LogType.Warning);
            return;
        }

        await streamWriter.WriteLineAsync(json);
        await streamWriter.FlushAsync();
    }

    /// <summary>
    /// Warning. This method can send data that is not in the form of packages, and can therefor cause issues for the
    /// receiver. Only use if you know what you are doing.
    /// </summary>
    /// <param name="json"></param>
    public async Task SendJson(string? json)
    {
        if (json == null)
        {
            logger.Log(this, "Result Json was null", LogType.Warning);
            return;
        }

        logger.Log(this, $"Sent pre-made json: {json}", LogType.Information);
        await streamWriter.WriteLineAsync(json);
        await streamWriter.FlushAsync();
    }

    // TODO: Does this task need cancellation?
    private async Task PackageListener()
    { 
        logger.Log(this, "PackageListener started", LogType.Information);
        while (true)
        {
            try
            {
                var json = await streamReader.ReadLineAsync();
                
                logger.Log(this, $"Received json: {json}", LogType.Information);
                
                if (json == null)
                {
                    logger.Log(this, $"Received json was null, disconnecting...", LogType.HandledError);
                    ClientDisconnected?.Invoke(this, new ClientDisconnectedEventArgs(true));
                    TcpClient.Close();
                    return;
                }

                var package = jsonSerializer.DeSerialize<Package>(json);
                if (package == null)
                {
                    logger.Log(this, $"Received package was null", LogType.HandledError);
                    continue;
                }

                var objectType = AppDomain.CurrentDomain.GetAssemblies()
                    .Select(assembly => assembly.GetType(package.Type))
                    .SingleOrDefault(type => type != null);

                if (objectType == null)
                {
                    logger.Log(this, $"Received package type does not exist in current context", LogType.HandledError);
                    continue;
                }

                var resultPackage = jsonSerializer.Deserialize(json, objectType) as Package;
                if (resultPackage == null)
                {
                    logger.Log(this,
                        $"Result Package was null. This should not happen, is something wrong with your IJsonSerializer class?", 
                        LogType.Warning);
                    continue;
                }

                PackageReceived?.Invoke(this, new PackageReceivedEventArgs(resultPackage, objectType, ConnectionInformation));
            }
            catch (IOException)
            {
                logger.Log(this, $"Client {ConnectionInformation} unexpectedly lost connection.", LogType.Information);
                Disconnect();
                return;
            }
            catch (Exception e)
            {
                logger.Log(this, $"An unexpected error has occured: {e}", LogType.Error);
                throw;
            }
        }
    }
}

/// <summary>
/// Event information for received packages.
/// </summary>
public class PackageReceivedEventArgs
{
    public PackageReceivedEventArgs(IPackage receivedPackage, Type packageType, IConnectionInformation connectionInformation)
    {
        ReceivedPackage = receivedPackage;
        PackageType = packageType;
        ConnectionInformation = connectionInformation;
    }
    public IPackage ReceivedPackage { get; }
    public Type PackageType { get; }
    public IConnectionInformation ConnectionInformation;
}

public class ClientDisconnectedEventArgs
{
    public bool RemoteDisconnected;
    public bool LocalDisconnected => !RemoteDisconnected;

    public ClientDisconnectedEventArgs(bool remoteDisconnected)
    {
        RemoteDisconnected = remoteDisconnected;
    }
}