using System.Net;
using System.Net.Sockets;

namespace Unnamed_Networking_Plugin;

public class Connection
{
    // public IPAddress ConnectedIp { get; }
    // public int ConnectedPort { get; }
    public TcpClient TcpClient { get; }
    public Stream DataStream { get; }


    public Connection(TcpClient tcpClient, Stream dataStream)
    {
        // ConnectedIp = connectedIp;
        // ConnectedPort = connectedPort;
        TcpClient = tcpClient;
        DataStream = dataStream;
    }
}