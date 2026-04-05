using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ChatCore.Interfaces;
using Proto;

namespace ChatApplication.Implementations.Transports;

/// <summary>
/// Proto Actor implementation of IMessagingTransport.
/// Actors coordinate message flow; UDP sockets handle actual network delivery.
/// </summary>
public sealed class ProtoActorTransport : IMessagingTransport
{
    private ActorSystem? _actorSystem;
    private PID? _serverActor;
    private PID? _clientActor;

    private UdpClient? _receiveClient;
    private UdpClient? _sendClient;
    private IPEndPoint? _remoteEndPoint;
    private CancellationTokenSource? _receiveCts;

    private bool _isListening;
    private bool _isConnected;

    public string Protocol => "ProtoActor";
    public bool IsListening => _isListening;
    public bool IsConnected => _isConnected;

    public event Action<string>? DebugMessage;
    public event Action<string, string>? MessageReceived;

    public void StartServer(string localIp, int localPort)
    {
        try
        {
            _actorSystem ??= new ActorSystem();

            // Spawn server actor
            _serverActor = _actorSystem.Root.Spawn(
                Props.FromProducer(() => new ServerActor(this))
            );

            // Bind UDP socket for receiving
            _receiveClient?.Dispose();
            _receiveClient = new UdpClient(new IPEndPoint(IPAddress.Parse(localIp), localPort));
            _receiveCts = new CancellationTokenSource();
            _ = ReceiveLoopAsync(_receiveCts.Token);

            _isListening = true;
            DebugMessage?.Invoke($"Proto Actor server started on {localIp}:{localPort}");
        }
        catch (Exception ex)
        {
            DebugMessage?.Invoke($"Error starting Proto Actor server: {ex.Message}");
        }
    }

    public void StopServer()
    {
        try
        {
            _receiveCts?.Cancel();
            _receiveClient?.Dispose();
            _receiveClient = null;

            if (_serverActor is not null && _actorSystem is not null)
                _actorSystem.Root.Stop(_serverActor);

            _isListening = false;
            DebugMessage?.Invoke("Proto Actor server stopped.");
        }
        catch (Exception ex)
        {
            DebugMessage?.Invoke($"Error stopping Proto Actor server: {ex.Message}");
        }
    }

    public void Connect(string remoteIp, int remotePort)
    {
        try
        {
            _remoteEndPoint = new IPEndPoint(IPAddress.Parse(remoteIp), remotePort);

            _sendClient?.Dispose();
            _sendClient = new UdpClient();

            _actorSystem ??= new ActorSystem();

            if (_clientActor is not null)
                _actorSystem.Root.Stop(_clientActor);

            _clientActor = _actorSystem.Root.Spawn(
                Props.FromProducer(() => new ClientActor(this))
            );

            _isConnected = true;
            DebugMessage?.Invoke($"Proto Actor client configured for {remoteIp}:{remotePort}");
        }
        catch (Exception ex)
        {
            _isConnected = false;
            DebugMessage?.Invoke($"Error connecting Proto Actor client: {ex.Message}");
        }
    }

    public void Disconnect()
    {
        try
        {
            if (_clientActor is not null && _actorSystem is not null)
                _actorSystem.Root.Stop(_clientActor);

            _sendClient?.Dispose();
            _sendClient = null;
            _isConnected = false;
            DebugMessage?.Invoke("Proto Actor client disconnected.");
        }
        catch (Exception ex)
        {
            DebugMessage?.Invoke($"Error disconnecting Proto Actor client: {ex.Message}");
        }
    }

    public void Send(string message)
    {
        if (_clientActor is null || !_isConnected)
        {
            DebugMessage?.Invoke("Proto Actor: client is not connected. Call Connect() first.");
            return;
        }

        try
        {
            _actorSystem?.Root.Send(_clientActor, new SendMessageRequest { Message = message });
        }
        catch (Exception ex)
        {
            DebugMessage?.Invoke($"Error sending message: {ex.Message}");
        }
    }

    public void Dispose()
    {
        try
        {
            StopServer();
            Disconnect();
            _actorSystem = null;
        }
        catch (Exception ex)
        {
            DebugMessage?.Invoke($"Error disposing Proto Actor transport: {ex.Message}");
        }
    }

    // ── UDP receive loop ──────────────────────────────────────────────

    private async Task ReceiveLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var result = await _receiveClient!.ReceiveAsync(ct);
                var text = Encoding.UTF8.GetString(result.Buffer).Trim().TrimEnd('\0');
                if (text.Length == 0) continue;

                var remote = result.RemoteEndPoint.ToString();
                DebugMessage?.Invoke($"Proto Actor RX {result.Buffer.Length} bytes from {remote}");

                if (_serverActor is not null && _actorSystem is not null)
                {
                    _actorSystem.Root.Send(_serverActor, new ReceiveMessageRequest
                    {
                        Message = text,
                        RemoteEndpoint = remote
                    });
                }
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                if (!ct.IsCancellationRequested)
                    DebugMessage?.Invoke($"Proto Actor receive error: {ex.Message}");
            }
        }
    }

    // ── Internal actor messages ───────────────────────────────────────

    internal record SendMessageRequest
    {
        public string Message { get; set; } = "";
    }

    internal record ReceiveMessageRequest
    {
        public string Message { get; set; } = "";
        public string RemoteEndpoint { get; set; } = "";
    }

    // ── Server Actor ──────────────────────────────────────────────────

    private sealed class ServerActor : IActor
    {
        private readonly ProtoActorTransport _parent;

        public ServerActor(ProtoActorTransport parent) => _parent = parent;

        public Task ReceiveAsync(IContext context)
        {
            return context.Message switch
            {
                ReceiveMessageRequest req => HandleReceiveMessage(req),
                _ => Task.CompletedTask
            };
        }

        private Task HandleReceiveMessage(ReceiveMessageRequest req)
        {
            try
            {
                _parent.MessageReceived?.Invoke(req.Message, req.RemoteEndpoint);
            }
            catch (Exception ex)
            {
                _parent.DebugMessage?.Invoke($"Server error processing message: {ex.Message}");
            }
            return Task.CompletedTask;
        }
    }

    // ── Client Actor ──────────────────────────────────────────────────

    private sealed class ClientActor : IActor
    {
        private readonly ProtoActorTransport _parent;

        public ClientActor(ProtoActorTransport parent) => _parent = parent;

        public Task ReceiveAsync(IContext context)
        {
            return context.Message switch
            {
                SendMessageRequest req => HandleSendMessage(req),
                _ => Task.CompletedTask
            };
        }

        private Task HandleSendMessage(SendMessageRequest req)
        {
            try
            {
                if (_parent._sendClient is null || _parent._remoteEndPoint is null)
                {
                    _parent.DebugMessage?.Invoke("Proto Actor: no send client or remote endpoint.");
                    return Task.CompletedTask;
                }

                var bytes = Encoding.UTF8.GetBytes(req.Message);
                _parent._sendClient.Send(bytes, bytes.Length, _parent._remoteEndPoint);
                _parent.DebugMessage?.Invoke($"Sent (Proto Actor) → {_parent._remoteEndPoint}");
            }
            catch (Exception ex)
            {
                _parent.DebugMessage?.Invoke($"Client error sending message: {ex.Message}");
            }
            return Task.CompletedTask;
        }
    }
}
