using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using hakoniwa.environment.impl;

namespace hakoniwa.environment.interfaces
{
    public class UDPCommunicationService : ICommunicationService
    {
        private readonly int localPort;
        private readonly string remoteAddress;
        private readonly int remotePort;
        private UdpClient udpClient;
        private CancellationTokenSource cancellationTokenSource;
        private Task receiveTask;
        private ICommunicationBuffer buffer;
        private bool isServiceEnabled = false;

        public UDPCommunicationService(int localPort, string remoteAddress, int remotePort)
        {
            this.localPort = localPort;
            this.remoteAddress = remoteAddress;
            this.remotePort = remotePort;
        }
        public bool StartService(ICommunicationBuffer comm_buffer)
        {
            if (isServiceEnabled)
            {
                return false;
            }
            buffer = comm_buffer;
            udpClient = new UdpClient(localPort);
            cancellationTokenSource = new CancellationTokenSource();
            receiveTask = Task.Run(() => ReceiveDataLoop(cancellationTokenSource.Token));
            isServiceEnabled = true;
            return true;
        }

        public bool StopService()
        {
            if (!isServiceEnabled)
            {
                return false;
            }

            cancellationTokenSource?.Cancel();
            // ソケットを閉じてReceiveAsyncを強制終了させる
            udpClient?.Client.Close();            
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
                udpClient?.Dispose();
                cancellationTokenSource?.Dispose();
                isServiceEnabled = false;
            }
            return true;
        }

        public bool SendData(string robotName, int channelId, byte[] pdu_data)
        {
            if (!isServiceEnabled)
            {
                return false;
            }
            IDataPacket packet = new DataPacket()
            {
                RobotName = robotName,
                ChannelId = channelId,
                BodyData = pdu_data
            };
            var endPoint = new IPEndPoint(IPAddress.Parse(remoteAddress), remotePort);
            var data = packet.Encode();
            udpClient?.Send(data, data.Length, endPoint);
            return true;
        }

        private async Task ReceiveDataLoop(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var result = await udpClient.ReceiveAsync();
                    var receivedData = result.Buffer;
                    // 受信データの処理
                    //Console.WriteLine($"Received Data: {BitConverter.ToString(receivedData)}");

                    IDataPacket packet = DataPacket.Decode(receivedData);
                    buffer.PutPacket(packet);

                }
                catch (SocketException e)
                {
                    Console.WriteLine($"Socket Exception: {e.Message}");
                }
            }
        }

        public bool IsServiceEnabled()
        {
            return isServiceEnabled;
        }
    }
}
