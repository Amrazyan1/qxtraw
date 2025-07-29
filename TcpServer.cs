using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System; // For EventHandler

public class TcpServer : IDisposable
{
    private readonly TcpListener _listener;
    private StreamWriter? _writer;
    private StreamReader? _reader;
    private TcpClient? _client;

    public event EventHandler<string>? OnMessageReceived;
    public event EventHandler? OnClientDisconnected; // 🔹 Added event

    private CancellationTokenSource _cts = new();

    public TcpServer(int port)
    {
        _listener = new TcpListener(IPAddress.Loopback, port);
        _listener.Start();
        Console.WriteLine($"Server listening on port {port}...");
    }

    public async Task StartAsync()
    {
        while (!_cts.Token.IsCancellationRequested)
        {
            Console.WriteLine("Waiting for Unity client...");
            _client = await _listener.AcceptTcpClientAsync();
            Console.WriteLine("🎮 Unity client connected!");

            var stream = _client.GetStream();
            _writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
            _reader = new StreamReader(stream, Encoding.UTF8);

            _ = Task.Run(ListenForIncomingMessages);
            await MonitorConnectionAsync();
        }
    }

    public async Task SendMessageAsync(string message)
    {
        if (_writer == null || _client == null || !_client.Connected)
        {
            Console.WriteLine("⚠ No client connected to send message.");
            return;
        }

        try
        {
            await _writer.WriteLineAsync(message);
            Console.WriteLine($"✅ Sent to Unity: {message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error sending message: {ex.Message}");
        }
    }

    private async Task ListenForIncomingMessages()
    {
        try
        {
            while (_client?.Connected == true)
            {
                string? message = await _reader?.ReadLineAsync();
                if (message == null) break; // Unity disconnected
                Console.WriteLine($"📧 Received from Client: {message}");
                OnMessageReceived?.Invoke(this, message);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading from client: {ex.Message}");
        }
        finally
        {
            HandleDisconnection();
        }
    }

    private async Task MonitorConnectionAsync()
    {
        while (_client?.Connected == true)
        {
            await Task.Delay(1000); // Check every second
        }
        HandleDisconnection();
    }

    private void HandleDisconnection()
    {
        Console.WriteLine("⚠ Unity client disconnected. Waiting for reconnection...");
        OnClientDisconnected?.Invoke(this, EventArgs.Empty);

        _client?.Close();
        _writer?.Dispose();
        _reader?.Dispose();
        _client = null;
        _writer = null;
        _reader = null;
    }

    public void Dispose()
    {
        Console.WriteLine("Disposing TcpServer...");
        _cts.Cancel();
        _client?.Close();
        _listener.Stop();
        _writer?.Dispose();
        _reader?.Dispose();
        Console.WriteLine("TcpServer disposed.");
    }
}
