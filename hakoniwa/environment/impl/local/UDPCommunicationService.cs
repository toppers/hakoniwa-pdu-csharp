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

        public UDPCommunicationService(int localPort, string remoteAddress, int remotePort)
        {
            this.localPort = localPort;
            this.remoteAddress = remoteAddress;
            this.remotePort = remotePort;
        }
        public void StartService(ICommunicationBuffer comm_buffer)
        {
            buffer = comm_buffer;
            udpClient = new UdpClient(localPort);
            cancellationTokenSource = new CancellationTokenSource();
            receiveTask = Task.Run(() => ReceiveDataLoop(cancellationTokenSource.Token));
        }

        public void StopService()
        {
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
                udpClient?.Close();
                udpClient?.Dispose();
                cancellationTokenSource?.Dispose();
            }
        }

        public void SendData(IDataPacket packet)
        {
            var endPoint = new IPEndPoint(IPAddress.Parse(remoteAddress), remotePort);
            var data = packet.Encode();
            udpClient?.Send(data, data.Length, endPoint);
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
                    Console.WriteLine($"Received Data: {BitConverter.ToString(receivedData)}");

                    IDataPacket packet = DataPacket.Decode(receivedData);
                    buffer.PutPacket(packet);

                }
                catch (SocketException e)
                {
                    Console.WriteLine($"Socket Exception: {e.Message}");
                }
                await Task.Delay(10); // 一定の間隔で受信処理を繰り返す
            }
        }

    }
}
