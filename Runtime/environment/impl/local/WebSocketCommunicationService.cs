using System;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using hakoniwa.environment.interfaces;

namespace hakoniwa.environment.impl.local
{
    public class WebSocketCommunicationService : ICommunicationService, IDisposable
    {
        private string serverUri;
        private readonly string packetVersion;
        private ClientWebSocket webSocket;
        private CancellationTokenSource cancellationTokenSource;
        private Task receiveTask;
        private ICommunicationBuffer buffer;
        private bool isServiceEnabled = false;
        private bool disposed = false;

        public string GetServerUri()
        {
            return serverUri;
        }

        public WebSocketCommunicationService(string serverUri, string packetVersion = "v1")
        {
            this.serverUri = serverUri;
            this.packetVersion = packetVersion;
        }

        public async Task<bool> StartService(ICommunicationBuffer comm_buffer, string uri = null)
        {
            if (isServiceEnabled)
            {
                return false;
            }

            buffer = comm_buffer;
            webSocket = new ClientWebSocket();
            cancellationTokenSource = new CancellationTokenSource();
            if (uri != null)
            {
                this.serverUri = uri;
            }

            try
            {
                await webSocket.ConnectAsync(new Uri(serverUri), cancellationTokenSource.Token);
                Console.WriteLine("WebSocket connection established (HTTP/2).");
            }
            catch (WebSocketException ex)
            {
                Console.WriteLine($"WebSocket connection error: {ex.Message}");
                isServiceEnabled = false;
                return false;
            }

            // 非同期で受信タスクを開始
            receiveTask = Task.Run(() => ReceiveData(cancellationTokenSource.Token));
            isServiceEnabled = true;
            return true;
        }

        private async Task ReceiveData(CancellationToken ct)
        {
            Console.WriteLine("Start ReceiveData...");
            while (!ct.IsCancellationRequested && webSocket.State == WebSocketState.Open)
            {
                try
                {
                    IDataPacket packet = await ReceivePacket(ct);
                    if (packet != null)
                    {
                        buffer.PutPacket(packet);
                    }
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Receive operation canceled.");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Receive error: {ex.Message}");
                    break;
                }
            }
        }

        private async Task<IDataPacket> ReceivePacket(CancellationToken ct)
        {
            var chunkBuffer = new byte[4096];
            using var stream = new System.IO.MemoryStream();

            while (!ct.IsCancellationRequested)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(chunkBuffer), ct);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, ct);
                    return null;
                }

                stream.Write(chunkBuffer, 0, result.Count);
                if (result.EndOfMessage)
                {
                    break;
                }
            }

            if (stream.Length == 0)
            {
                return null;
            }

            return DataPacket.Decode(stream.ToArray(), packetVersion);
        }

        public async Task<bool> SendData(string robotName, int channelId, byte[] pdu_data)
        {
            if (!isServiceEnabled || webSocket.State != WebSocketState.Open)
            {
                Console.WriteLine($"send error: isServiceEnabled={isServiceEnabled} webSocket.State ={webSocket.State}");
                return false;
            }

            var packet = new DataPacket()
            {
                RobotName = robotName,
                ChannelId = channelId,
                BodyData = pdu_data
            };

            var data = packet.Encode(packetVersion);

            try
            {
                await webSocket.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Binary, true, CancellationToken.None);
                Console.WriteLine($"Sending {data.Length} bytes to WebSocket.");
                return true;
            }
            catch (WebSocketException ex)
            {
                Console.WriteLine($"Failed to send data: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> StopService()
        {
            Console.WriteLine("Stop Service");
            if (!isServiceEnabled)
            {
                return false;
            }
            try
            {
                if (webSocket?.State == WebSocketState.Open)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Service stopping", CancellationToken.None);
                }

                cancellationTokenSource?.Cancel();

                if (receiveTask != null)
                {
                    await receiveTask;
                }
            }
            catch (AggregateException e)
            {
                Console.WriteLine($"Exception in StopService: {e.Message}");
            }
            finally
            {
                webSocket?.Dispose();
                cancellationTokenSource?.Dispose();
                isServiceEnabled = false;
            }

            return true;
        }

        public bool IsServiceEnabled()
        {
            return isServiceEnabled;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                // マネージリソースの解放
                if (isServiceEnabled)
                {
                    StopService().Wait();
                }
                webSocket?.Dispose();
                cancellationTokenSource?.Dispose();
            }

            disposed = true;
        }
    }
}
