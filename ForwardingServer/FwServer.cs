using Unnamed_Networking_Plugin;
using Unnamed_Networking_Plugin.EventArgs;
using Unnamed_Networking_Plugin.Interfaces;
using Unnamed_Networking_Plugin.Resources;

namespace ForwardingServer;

public class FwServer
{
    private int port { get; }
    private ILogger logger { get; }
    private IJsonSerializer jsonSerializer { get; }
    private IdentificationPackage identificationPackage { get; }
    private Type identificationType { get; }
    private ServerInterface serverInterface { get; set; }
    private UnnamedNetworkPluginClient client { get; set; }
    
    public bool Running { get; private set; }

    
    
    public FwServer(
        int port, 
        ILogger logger, 
        IJsonSerializer jsonSerializer, 
        IdentificationPackage identificationPackage)
    {
        this.port = port;
        this.logger = logger;
        this.jsonSerializer = jsonSerializer;
        this.identificationPackage = identificationPackage;
        identificationType = identificationPackage.ExtractConnectionInformation().GetType();
    }

    // TODO: This is strangely empty.
    public async Task Run()
    {
        if (Running)
        {
            logger.Log(this, "Run was called on an already running server. That does not work.", LogType.Error);
            throw new Exception("Run was called on an already running server. This is not allowed.");
        }
        
        Running = true;
        client = new UnnamedNetworkPluginClient(port, logger, jsonSerializer, identificationPackage);

        client.ConnectionSuccessful += ConnectionSuccessfulHandler;
        
        serverInterface = new ServerInterface(client, this);
    }

    public async Task Stop()
    {
        await client.StopListener();
    }

    private void ConnectionSuccessfulHandler(object? o, ConnectionReceivedEventArgs args)
    {
        PlaceConnectionInMenu(args.Connection);
    }

    public async void PlaceConnectionInMenu(Connection connection)
    {
        await serverInterface.PutInMenu(connection);
    }
}