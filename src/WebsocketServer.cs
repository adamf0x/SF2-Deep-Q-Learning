using System;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Socket
{
    public class SocketServer
    {
        private HttpListener? _httpListener;
        private WebSocket? _clientSocket;
        private CancellationTokenSource? _cts;
        private bool _isRunning = false;

        private string actionToPerform = "";

        public bool GetIsRunning()
        {
            return _isRunning;
        }

        public string GetActionToPerform()
        {
            return actionToPerform;
        }

        public async Task StartServer()
        {
            _cts = new CancellationTokenSource();
            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add("http://localhost:8080/");
            _httpListener.Start();
            _isRunning = true;

            // Run the accept loop in background
            _ = Task.Run(() => AcceptClient(_cts.Token));

            await Task.CompletedTask;
        }

        public async void SendToClient(string message)
        {
            if (_clientSocket?.State == WebSocketState.Open)
            {
                try
                {
                    byte[] buffer = Encoding.UTF8.GetBytes(message);
                    await _clientSocket.SendAsync(
                        new ArraySegment<byte>(buffer),
                        WebSocketMessageType.Text,
                        true,
                        CancellationToken.None
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public async Task StopServer()
        {
            _isRunning = false;
            _cts?.Cancel();

            // Close client connection if exists
            if (_clientSocket?.State == WebSocketState.Open)
            {
                try
                {
                    await _clientSocket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "Server shutting down",
                        CancellationToken.None
                    );
                }
                catch { }
                _clientSocket.Dispose();
                _clientSocket = null;
            }

            // Stop HTTP listener
            if (_httpListener?.IsListening == true)
            {
                _httpListener.Stop();
            }
            _httpListener?.Close();

            _cts?.Dispose();
            _cts = null;
        }

        private async Task AcceptClient(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested && _httpListener?.IsListening == true)
                {
                    var context = await _httpListener.GetContextAsync();

                    if (context.Request.IsWebSocketRequest)
                    {
                        if (_clientSocket == null || _clientSocket.State != WebSocketState.Open)
                        {
                            var wsContext = await context.AcceptWebSocketAsync(null);
                            _clientSocket = wsContext.WebSocket;

                            _ = ListenForMessages(_clientSocket, token);
                        }
                        else
                        {
                            context.Response.StatusCode = 503;
                            context.Response.Close();
                        }
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                        context.Response.Close();
                    }
                }
            }
            catch (HttpListenerException) when (token.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async Task ListenForMessages(WebSocket socket, CancellationToken token)
        {
            var buffer = new byte[1024 * 4];

            try
            {
                while (socket.State == WebSocketState.Open && !token.IsCancellationRequested)
                {
                    var result = await socket.ReceiveAsync(
                        new ArraySegment<byte>(buffer),
                        token
                    );
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        actionToPerform = message;
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await socket.CloseAsync(
                            WebSocketCloseStatus.NormalClosure,
                            "Closing",
                            CancellationToken.None
                        );
                        break;
                    }
                }
            }
            catch (WebSocketException)
            {
                // Client disconnected
            }
            catch (OperationCanceledException)
            {
                await StopServer();
            }
            finally
            {
                if (socket == _clientSocket)
                {
                    _clientSocket = null;
                }

                if (socket.State != WebSocketState.Closed && socket.State != WebSocketState.Aborted)
                {
                    await StopServer();
                }
                socket.Dispose();
            }
        }
    }
}
