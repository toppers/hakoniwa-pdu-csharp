using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using hakoniwa.environment.impl;
using hakoniwa.environment.interfaces;
using hakoniwa.pdu.interfaces;
using hakoniwa.pdu.core;

namespace hakoniwa.environment.test
{
    public class UDPCommunicationServiceTest
    {
        private readonly int senderPort = 11000;
        private readonly int receiverPort = 11001;
        private readonly string remoteAddress = "127.0.0.1";

        [Fact]
        public async Task Test_UDPCommunicationService_SendAndReceive()
        {
            // 受信サービスのセットアップ
            var receiverBuffer = new CommunicationBufferMock();
            var receiverService = new UDPCommunicationService(receiverPort, remoteAddress, senderPort);
            Console.WriteLine("INFO: Start Receiver service");
            receiverService.StartService(receiverBuffer);

            // 送信サービスのセットアップ
            var senderBuffer = new CommunicationBufferMock();
            var senderService = new UDPCommunicationService(senderPort, remoteAddress, receiverPort);
            Console.WriteLine("INFO: Start Sender service");
            senderService.StartService(senderBuffer);

            // 送信するデータの準備
            string robotName = "DroneTransporter";
            int channelId = 1;
            byte[] sendData = Encoding.UTF8.GetBytes("Hello UDP");

            // データを送信
            Console.WriteLine($"INFO: Send Data: {sendData}");
            var sendResult = senderService.SendData(robotName, channelId, sendData);
            Assert.True(sendResult, "Failed to send data.");

            // データが受信されるまで少し待機
            await Task.Delay(500);

            // 受信バッファの確認
            var receivedPacket = receiverBuffer.GetLatestPacket();
            Assert.NotNull(receivedPacket);
            Assert.Equal(robotName, receivedPacket.GetRobotName());
            Assert.Equal(channelId, receivedPacket.GetChannelId());
            Assert.Equal(sendData, receivedPacket.GetPduData());

            // サービスの停止
            Console.WriteLine("INFO: Stop Sender service");
            senderService.StopService();
            Console.WriteLine("INFO: Stop Recv service");
            receiverService.StopService();
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
