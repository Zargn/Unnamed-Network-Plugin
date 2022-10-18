using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;
using Unnamed_Networking_Plugin.Interfaces;

namespace Unnamed_Networking_Plugin;

public class Connection
{
    public TcpClient TcpClient { get; }
    public Stream DataStream { get; }
    
    public event EventHandler<PackageReceivedEventArgs>? PackageReceived;
    public event EventHandler<ClientDisconnectedEventArgs>? ClientDisconnected;

    private StreamReader streamReader { get; }
    private StreamWriter streamWriter { get; }

    private IJsonSerializer jsonSerializer;
    private ILogger logger;

    private Task packageListenerTask;


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
        throw new NotImplementedException();
    }

    public async Task SendPackage<T>(T package)
    where T : IPackage
    {
        var json = jsonSerializer.Serialize<T>(package);
        if (json == null)
        {
            logger.Log(this, "Result Json was null", LogType.Warning);
            return;
        }

        await streamWriter.WriteLineAsync(json);
        await streamWriter.FlushAsync();
    }

    public async Task PackageListener()
    { 
        logger.Log(this, "PackageListener started", LogType.Information);
        while (true)
        {
            try
            {
                var json = await streamReader.ReadLineAsync();
                if (json == null)
                {
                    logger.Log(this, $"Received json was null", LogType.HandledError);
                    continue;
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
                    logger.Log(this, $"Result Package was null. This should not happen, is something wrong with your IJsonSerializer class?", LogType.Warning);
                    continue;
                }
                
                PackageReceived?.Invoke(this, new PackageReceivedEventArgs(resultPackage));
            }
            catch (Exception e)
            {
                logger.Log(this, $"And unexpected error has occured: {e}", LogType.Error);
                // TODO: Should I maybe not throw here? Do I want the entire loop to stop if a invalid JSON is received?
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
    public PackageReceivedEventArgs(IPackage receivedPackage)
    {
        ReceivedPackage = receivedPackage;
    }
    public IPackage ReceivedPackage { get; }
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