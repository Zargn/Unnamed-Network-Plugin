using System.Net;
using System.Net.Sockets;
using Unnamed_Networking_Plugin.Interfaces;

namespace Unnamed_Networking_Plugin;

public class Connection
{
    // public IPAddress ConnectedIp { get; }
    // public int ConnectedPort { get; }
    public TcpClient TcpClient { get; }
    public Stream DataStream { get; }

    private StreamReader streamReader { get; }
    private StreamWriter streamWriter { get; }

    private IJsonSerializer jsonSerializer;
    private ILogger logger;


    public Connection(TcpClient tcpClient, Stream dataStream, IJsonSerializer jsonSerializer, ILogger logger)
    {
        // ConnectedIp = connectedIp;
        // ConnectedPort = connectedPort;
        TcpClient = tcpClient;
        DataStream = dataStream;
        this.jsonSerializer = jsonSerializer;
        this.logger = logger;
        streamReader = new StreamReader(dataStream);
        streamWriter = new StreamWriter(dataStream);
    }

    public async Task<IPackage> ReceivePackage()
    {
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
                
                return resultPackage;
            }
            catch (Exception e)
            {
                logger.Log(this, $"And unexpected error has occured: {e}", LogType.Error);
                throw;
            }
        }
    }
}