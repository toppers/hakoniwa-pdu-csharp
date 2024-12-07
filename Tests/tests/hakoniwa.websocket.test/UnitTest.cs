using System;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using hakoniwa.environment.impl.local;
using hakoniwa.environment.interfaces;

namespace hakoniwa.environment.test
{
    public class WebSocketCommunicationServiceTest
    {
        private readonly string serverUri = "ws://localhost:8080/echo";
        private HttpListener? listener;

        private async Task StartWebSocketServer(CancellationToken cancellationToken)
        {
            listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:8080/echo/");
            listener.Start();
            Console.WriteLine("WebSocket Test Server started.");

            _ = Task.Run(async () =>
            {
                while (listener.IsListening && !cancellationToken.IsCancellationRequested)
                {
                    var context = await listener.GetContextAsync();
                    if (context.Request.IsWebSocketRequest)
                    {
                        var webSocketContext = await context.AcceptWebSocketAsync(null);
                        await EchoMessages(webSocketContext.WebSocket, cancellationToken);
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                        context.Response.Close();
                    }
                }
            }, cancellationToken);

            // サーバーがリッスン状態になるまでの待機を確認し、リスニング状態を出力
            while (!listener.IsListening)
            {
                await Task.Delay(100);  // 少しの遅延を繰り返し、リッスン状態を確認
            }
            Console.WriteLine("Server is now fully listening.");
        }

        private async Task EchoMessages(WebSocket webSocket, CancellationToken cancellationToken)
        {
            var buffer = new byte[1024];
            while (webSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", cancellationToken);
                }
                else
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine($"Server received message: {message}");
                    await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(message)),
                        WebSocketMessageType.Text, true, cancellationToken);
                }
            }
        }

        private void StopWebSocketServer()
        {
            listener?.Stop();
            listener?.Close();
            Console.WriteLine("WebSocket Server stopped.");
        }

        [Fact]
        public async Task Test_WebSocketCommunicationService_Echo()
        {
            using var cts = new CancellationTokenSource();
            await StartWebSocketServer(cts.Token);

            var buffer = new CommunicationBufferMock();
            var webSocketService = new WebSocketCommunicationService(serverUri);
            var result = await webSocketService.StartService(buffer);
            Assert.True(result, "Failed websocket start");

            // 接続が確立されるのを確認
            await Task.Delay(500);

            // 送信するデータの準備
            string robotName = "DroneTransporter";
            int channelId = 1;
            byte[] sendData = Encoding.UTF8.GetBytes("Hello WebSocket");

            // データを送信
            var sendResult = await webSocketService.SendData(robotName, channelId, sendData);
            Assert.True(sendResult, "Failed to send data.");

            // エコーバックメッセージが正しく受信されるのを待機
            var receivedPacket = await WaitForEcho(buffer);
            Assert.NotNull(receivedPacket);
            Assert.Equal(robotName, receivedPacket.GetRobotName());
            Assert.Equal(channelId, receivedPacket.GetChannelId());
            Assert.Equal(sendData, receivedPacket.GetPduData());

            // WebSocketサービスとサーバーを停止
            await webSocketService.StopService();
            StopWebSocketServer();
        }

        private async Task<IDataPacket?> WaitForEcho(CommunicationBufferMock buffer, int timeout = 3000)
        {
            var endTime = DateTime.Now.AddMilliseconds(timeout);
            while (DateTime.Now < endTime)
            {
                var packet = buffer.GetLatestPacket();
                if (packet != null)
                {
                    return packet;
                }
                await Task.Delay(100);
            }
            return null;
        }
    }

    public class CommunicationBufferMock : ICommunicationBuffer
    {
        private IDataPacket? latestPacket;
        public string GetKey(string robotName, string pduName)
        {
            return robotName + "&" + pduName;
        }
        public void Key2RobotPdu(string key, out string robotName, out string pduName)
        {
            string[] tokens = key.Split('&');
            if (tokens.Length != 2)
            {
                throw new ArgumentException("Invalid key format");
            }
            robotName = tokens[0];
            pduName = tokens[1];
        }

        public void PutPacket(IDataPacket packet)
        {
            latestPacket = packet;
        }
        public void PutPacket(string robotName, int channelId, byte[] pdu_data)
        {
            throw new NotImplementedException();
        }

        public IDataPacket? GetLatestPacket()
        {
            return latestPacket;
        }
    }
}
