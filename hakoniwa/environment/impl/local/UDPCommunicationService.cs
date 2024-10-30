using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

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

        public UDPCommunicationService(int localPort, string remoteAddress, int remotePort)
        {
            this.localPort = localPort;
            this.remoteAddress = remoteAddress;
            this.remotePort = remotePort;
        }

        public void StartService()
        {
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

        public void SendData(byte[] data)
        {
            var endPoint = new IPEndPoint(IPAddress.Parse(remoteAddress), remotePort);
            udpClient?.Send(data, data.Length, endPoint);
        }

        public byte[] ReceiveData()
        {
            return null; // 非同期受信で利用されるので、ここでは直接の返却はしない
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
