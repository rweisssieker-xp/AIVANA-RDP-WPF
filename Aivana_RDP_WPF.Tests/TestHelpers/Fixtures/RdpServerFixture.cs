using System.Net;
using System.Net.Sockets;

namespace Aivana_RDP_WPF.Tests.TestHelpers.Fixtures;

/// <summary>
/// Mock/test RDP server fixture for integration tests
/// </summary>
public class RdpServerFixture : IDisposable
{
    private TcpListener? _listener;
    private bool _isRunning;
    private readonly int _port;

    public RdpServerFixture(int port = 3389)
    {
        _port = port;
    }

    public void Start()
    {
        _listener = new TcpListener(IPAddress.Loopback, _port);
        _listener.Start();
        _isRunning = true;
    }

    public void Stop()
    {
        _isRunning = false;
        _listener?.Stop();
    }

    public bool IsRunning => _isRunning;
    public int Port => _port;

    public void Dispose()
    {
        Stop();
        _listener?.Stop();
    }
}

