using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xunit;
using Fleck;
using hakoniwa.environment.impl.local;
using hakoniwa.environment.interfaces;

namespace hakoniwa.environment.test
{
    public class WebSocketCommunicationServiceTest
    {
        private readonly string serverUri = "ws://127.0.0.1:8080/echo";
        private WebSocketServer? server;
        private readonly List<IWebSocketConnection> connectedClients = new List<IWebSocketConnection>();

        private async Task StartWebSocketServer()
        {
            FleckLog.Level = LogLevel.Info;
            server = new WebSocketServer("ws://127.0.0.1:8080");
            Console.WriteLine("Start WebSocket Test Server");

            server.Start(socket =>
            {
                socket.OnOpen = () => OnOpen(socket);
                socket.OnClose = () => OnClose(socket);
                socket.OnMessage = message => OnMessage(socket, message);
            });
            await Task.Delay(500);
            Console.WriteLine("WebSocket Test Server started.");
        }

        private void StopWebSocketServer()
        {
            lock (connectedClients)
            {
                foreach (var socket in connectedClients)
                {
                    if (socket.IsAvailable)
                    {
                        socket.Close();
                    }
                }
                connectedClients.Clear();
            }
            server?.Dispose();
            Console.WriteLine("WebSocket Server stopped.");
        }

        private void OnOpen(IWebSocketConnection socket)
        {
            lock (connectedClients)
            {
                connectedClients.Add(socket);
            }
            Console.WriteLine($"Client connected: {socket.ConnectionInfo.ClientIpAddress}:{socket.ConnectionInfo.ClientPort}");
        }

        private void OnClose(IWebSocketConnection socket)
        {
            lock (connectedClients)
            {
                connectedClients.Remove(socket);
            }
            Console.WriteLine($"Client disconnected: {socket.ConnectionInfo.ClientIpAddress}:{socket.ConnectionInfo.ClientPort}");
        }

        private void OnMessage(IWebSocketConnection socket, string message)
        {
            Console.WriteLine($"Server received message: {message}");
            socket.Send(message); // エコーバック
        }

        [Fact]
        public async Task Test_WebSocketCommunicationService_Echo()
        {
            // WebSocketサーバーを開始
            await StartWebSocketServer();

            // WebSocketクライアントの設定
            var buffer = new CommunicationBufferMock();
            var webSocketService = new WebSocketCommunicationService(serverUri);
            Console.WriteLine("INFO: Start WebSocket service");
            webSocketService.StartService(buffer);
            await Task.Delay(3000); // サーバーとの接続の確立を待機

            // 送信するデータの準備
            string robotName = "DroneTransporter";
            int channelId = 1;
            byte[] sendData = Encoding.UTF8.GetBytes("Hello WebSocket");

            // データを送信
            Console.WriteLine($"INFO: Sending Data: {Encoding.UTF8.GetString(sendData)}");
            var sendResult = await webSocketService.SendData(robotName, channelId, sendData);
            Assert.True(sendResult, "Failed to send data.");

            // エコーバックメッセージを待機
            await Task.Delay(3000);

            // エコーバックメッセージが正しく受信されたか確認
            var receivedPacket = buffer.GetLatestPacket();
            Assert.NotNull(receivedPacket);
            Assert.Equal(robotName, receivedPacket.GetRobotName());
            Assert.Equal(channelId, receivedPacket.GetChannelId());
            Assert.Equal(sendData, receivedPacket.GetPduData());

            // WebSocketサービスとサーバーを停止
            Console.WriteLine("INFO: Stop WebSocket service");
            webSocketService.StopService();
            StopWebSocketServer();
            Console.WriteLine("INFO: DONE");
        }
    }

    // テスト用のバッファモック
    public class CommunicationBufferMock : ICommunicationBuffer
    {
        private IDataPacket? latestPacket;

        public void PutPacket(IDataPacket packet)
        {
            latestPacket = packet;
        }

        public IDataPacket? GetLatestPacket()
        {
            return latestPacket;
        }
    }
}
