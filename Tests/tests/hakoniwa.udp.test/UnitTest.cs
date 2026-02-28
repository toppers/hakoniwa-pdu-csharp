using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using hakoniwa.environment.impl.local;
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
            var ret = await receiverService.StartService(receiverBuffer);
            Assert.True(ret, "StartService is Failed");

            // 送信サービスのセットアップ
            var senderBuffer = new CommunicationBufferMock();
            var senderService = new UDPCommunicationService(senderPort, remoteAddress, receiverPort);
            Console.WriteLine("INFO: Start Sender service");
            ret = await senderService.StartService(senderBuffer);
            Assert.True(ret, "StartService is Failed");

            // 送信するデータの準備
            string robotName = "DroneTransporter";
            int channelId = 1;
            byte[] sendData = Encoding.UTF8.GetBytes("Hello UDP");

            // データを送信
            Console.WriteLine($"INFO: Send Data: {sendData}");
            var sendResult = await senderService.SendData(robotName, channelId, sendData);
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
            await senderService.StopService();
            Console.WriteLine("INFO: Stop Recv service");
            await receiverService.StopService();
            Console.WriteLine("INFO: DONE");
        }

        [Fact]
        public async Task Test_UDPCommunicationService_SendAndReceive_V2()
        {
            int v2SenderPort = 11010;
            int v2ReceiverPort = 11011;

            var receiverBuffer = new CommunicationBufferMock();
            var receiverService = new UDPCommunicationService(v2ReceiverPort, remoteAddress, v2SenderPort, "v2");
            var ret = await receiverService.StartService(receiverBuffer);
            Assert.True(ret, "StartService is Failed");

            var senderBuffer = new CommunicationBufferMock();
            var senderService = new UDPCommunicationService(v2SenderPort, remoteAddress, v2ReceiverPort, "v2");
            ret = await senderService.StartService(senderBuffer);
            Assert.True(ret, "StartService is Failed");

            string robotName = "Drone-1";
            int channelId = 7;
            byte[] sendData = Encoding.UTF8.GetBytes("Hello UDP v2");

            var sendResult = await senderService.SendData(robotName, channelId, sendData);
            Assert.True(sendResult, "Failed to send data.");

            await Task.Delay(500);

            var receivedPacket = receiverBuffer.GetLatestPacket();
            Assert.NotNull(receivedPacket);
            Assert.Equal(robotName, receivedPacket.GetRobotName());
            Assert.Equal(channelId, receivedPacket.GetChannelId());
            Assert.Equal(sendData, receivedPacket.GetPduData());

            await senderService.StopService();
            await receiverService.StopService();
        }
    }

    // テスト用のバッファモック
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
