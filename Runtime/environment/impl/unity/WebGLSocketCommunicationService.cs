
#if UNITY_WEBGL
using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using hakoniwa.environment.interfaces;

namespace hakoniwa.environment.impl.unity
{
    public class WebGLSocketCommunicationService : ICommunicationService
    {
        private readonly string serverUri;
        private ClientWebSocket webSocket;
        private CancellationTokenSource cancellationTokenSource;
        private Task receiveTask;
        private ICommunicationBuffer buffer;
        private bool isServiceEnabled = false;
        public string GetServerUri()
        {
            return serverUri;
        }

        public WebGLSocketCommunicationService(string serverUri)
        {
            this.serverUri = serverUri;
        }

        public async Task<bool> StartService(ICommunicationBuffer comm_buffer)
        {
            if (isServiceEnabled)
            {
                return false;
            }

            buffer = comm_buffer;
            webSocket = new ClientWebSocket();
            cancellationTokenSource = new CancellationTokenSource();


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
            var headerBuffer = new byte[4];
            Console.WriteLine("Start ReceiveData...");
            while (!ct.IsCancellationRequested && webSocket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult headerResult;
                try
                {
                    headerResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(headerBuffer), ct);
                    //Console.WriteLine($"headerResult: {headerResult.Count}");
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

                if (headerResult.Count < 4)
                {
                    Console.WriteLine($"Header size is less than expected: {headerResult.Count}");
                    continue;
                }

                int totalLength = BitConverter.ToInt32(headerBuffer, 0);
                var dataBuffer = new byte[4 + totalLength];
                Array.Copy(headerBuffer, 0, dataBuffer, 0, 4);

                int bytesRead = 0;
                while (bytesRead < totalLength && !ct.IsCancellationRequested)
                {
                    try
                    {
                        var segment = new ArraySegment<byte>(dataBuffer, 4 + bytesRead, totalLength - bytesRead);
                        WebSocketReceiveResult result = await webSocket.ReceiveAsync(segment, ct);
                        bytesRead += result.Count;
                        //Console.WriteLine($"bytesRead: {bytesRead} totalLength: {totalLength}");

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, ct);
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error receiving data: {ex.Message}");
                        break;
                    }
                }

                if (bytesRead == totalLength)
                {
                    IDataPacket packet = DataPacket.Decode(dataBuffer);
                    buffer.PutPacket(packet);
                    //Console.WriteLine("Data received and processed.");
                }
            }
        }

        public async Task<bool> SendData(string robotName, int channelId, byte[] pdu_data)
        {
            if (!isServiceEnabled || webSocket.State != WebSocketState.Open)
            {
                Console.WriteLine($"send error: isServiceEnabled={isServiceEnabled} webSocket.State ={webSocket.State }");
                return false;
            }

            IDataPacket packet = new DataPacket()
            {
                RobotName = robotName,
                ChannelId = channelId,
                BodyData = pdu_data
            };

            var data = packet.Encode();

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

        public bool StopService()
        {
            Console.WriteLine("Stop Service");
            if (!isServiceEnabled)
            {
                return false;
            }

            if (webSocket.State == WebSocketState.Open)
            {
                webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Service stopping", CancellationToken.None).Wait();
            }

            cancellationTokenSource?.Cancel();

            try
            {
                receiveTask?.Wait();
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
    }
}
#endif