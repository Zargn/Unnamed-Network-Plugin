using System.Net.Sockets;
using Unnamed_Networking_Plugin.Interfaces;
using Unnamed_Networking_Plugin.Packages;

namespace Unnamed_Networking_Plugin;

public class Connection
{
    public TcpClient TcpClient { get; }
    public Stream DataStream { get; }
    
    public event EventHandler<PackageReceivedEventArgs>? PackageReceived;
    public event EventHandler<ClientDisconnectedEventArgs>? ClientDisconnected;
    
    
    // TODO: This property should probably be moved out of this class since it is only useful if you use the full app.
    public IConnectionInformation ConnectionInformation { get; set; }

    
    private StreamReader streamReader { get; }
    private StreamWriter streamWriter { get; }

    private IJsonSerializer jsonSerializer;
    private ILogger logger;

    private Task packageListenerTask;

    private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();


    public Connection(TcpClient tcpClient, Stream dataStream, IJsonSerializer jsonSerializer, ILogger logger)
    {
        TcpClient = tcpClient;
        DataStream = dataStream;
        this.jsonSerializer = jsonSerializer;
        this.logger = logger;
        
        streamReader = new StreamReader(dataStream);
        streamWriter = new StreamWriter(dataStream);
        
        logger.Log(this, "Connection created", LogType.Information);
        
        packageListenerTask = PackageListener();
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
        if (json == null)
        {
            logger.Log(this, "Result Json was null", LogType.Warning);
            return;
        }

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

                PackageReceived?.Invoke(this, new PackageReceivedEventArgs(resultPackage, objectType));
            }
            catch (IOException)
            {
                logger.Log(this, $"Client {ConnectionInformation} lost connection.", LogType.Information);
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
    public PackageReceivedEventArgs(IPackage receivedPackage, Type packageType)
    {
        ReceivedPackage = receivedPackage;
        PackageType = packageType;
    }
    public IPackage ReceivedPackage { get; }
    public Type PackageType { get; }
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