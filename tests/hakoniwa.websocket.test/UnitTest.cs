using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp.Server; // WebSocketSharpのサーバー機能
using Xunit;
using hakoniwa.environment.impl;
using hakoniwa.environment.interfaces;

namespace hakoniwa.environment.test
{
    public class WebSocketCommunicationServiceTest
    {
        private readonly string serverUri = "ws://localhost:8080/echo";

        private void StartWebSocketEchoServer()
        {
            // WebSocketSharpを用いてサーバーを立ち上げる
            var wssv = new WebSocketServer(8080);
            wssv.AddWebSocketService<EchoBehavior>("/echo");
            wssv.Start();
            Console.WriteLine("Start WebSocket Test Server");

            // テスト終了後のサーバー停止
            AppDomain.CurrentDomain.ProcessExit += (s, e) => wssv.Stop();
        }

        [Fact]
        public async Task Test_WebSocketCommunicationService_Echo()
        {
            StartWebSocketEchoServer();

            var buffer = new CommunicationBufferMock();
            var webSocketService = new WebSocketCommunicationService(serverUri);
            Console.WriteLine("INFO: Start WebSocket service");
            webSocketService.StartService(buffer);
            await Task.Delay(500); // 接続の確立を待機

            // 送信するデータの準備
            string robotName = "DroneTransporter";
            int channelId = 1;
            byte[] sendData = Encoding.UTF8.GetBytes("Hello WebSocket");

            // データを送信
            Console.WriteLine($"INFO: Sending Data: {Encoding.UTF8.GetString(sendData)}");
            var sendResult = await webSocketService.SendData(robotName, channelId, sendData);
            Assert.True(sendResult, "Failed to send data.");

            // データがエコーバックされるまで少し待機
            await Task.Delay(10000);

            // 受信バッファの確認
            var receivedPacket = buffer.GetLatestPacket();
            Assert.NotNull(receivedPacket);
            Assert.Equal(robotName, receivedPacket.GetRobotName());
            Assert.Equal(channelId, receivedPacket.GetChannelId());
            Assert.Equal(sendData, receivedPacket.GetPduData());

            // サービスの停止
            Console.WriteLine("INFO: Stop WebSocket service");
            webSocketService.StopService();
            Console.WriteLine("INFO: DONE");
        }
    }

    // WebSocketSharpのエコー動作を定義
    public class EchoBehavior : WebSocketBehavior
    {
        protected override void OnMessage(WebSocketSharp.MessageEventArgs e)
        {
            Console.WriteLine("Server received: " + e.Data);
            Send(e.Data); // エコーバック
            Console.WriteLine("echo back! ");
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
