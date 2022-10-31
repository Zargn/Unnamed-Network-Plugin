using Unnamed_Networking_Plugin;
using Unnamed_Networking_Plugin.Interfaces;
using Unnamed_Networking_Plugin.Resources;

namespace ForwardingServer;

public class FwServer
{
    public FwServer(int port, ILogger logger, IJsonSerializer jsonSerializer, IdentificationPackage identificationPackage)
    {
        var client = new UnnamedNetworkPluginClient(port, logger, jsonSerializer, identificationPackage);
    }

    public async Task Run()
    {
        
    }

    public void Stop()
    {
        
    }
}