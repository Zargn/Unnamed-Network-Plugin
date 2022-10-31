using Unnamed_Networking_Plugin;
using Unnamed_Networking_Plugin.Interfaces;
using Unnamed_Networking_Plugin.Resources;

namespace ForwardingServer;

public class FwServer
{
    private int port { get; }
    private ILogger logger { get; }
    private IJsonSerializer jsonSerializer { get; }
    private IdentificationPackage identificationPackage { get; }
    public bool Running { get; private set; }

    public FwServer(int port, ILogger logger, IJsonSerializer jsonSerializer, IdentificationPackage identificationPackage)
    {
        this.port = port;
        this.logger = logger;
        this.jsonSerializer = jsonSerializer;
        this.identificationPackage = identificationPackage;
    }

    public async Task Run()
    {
        if (Running)
        {
            logger.Log(this, "Run was called on an already running server. That does not work.", LogType.Error);
            throw new Exception("Run was called on an already running server. This is not allowed.")
        }
        
        Running = true;
        var client = new UnnamedNetworkPluginClient(port, logger, jsonSerializer, identificationPackage);
    }

    public async Task Stop()
    {
        
    }
}