using System;
using hakoniwa.environment.impl.local;
using hakoniwa.environment.interfaces;
using Newtonsoft.Json;

namespace hakoniwa.environment.impl
{
    public class UdpConfig
    {
        public int LocalPort { get; set; }
        public int RemotePort { get; set; }
        public string RemoteIPAddress { get; set; }
    }

    public class WebSocketConfig
    {
        public string ServerURI { get; set; }
    }

    public class CommServiceConfig
    {
        public UdpConfig Udp { get; set; }
        public WebSocketConfig WebSocket { get; set; }
    }    
    public class EnvironmentServiceFactory
    {
        static public IEnvironmentService Create(string service_type)
        {
            return new EnvironmentService(service_type);
        }
    }
    public class EnvironmentService : IEnvironmentService
    {
        IFileLoader file_loader;
        ICommunicationService comm_service;

        private CommServiceConfig loadCommServiceConfig()
        {
            string param = file_loader.LoadText("comm_service_config", ".json");

            try
            {
                CommServiceConfig config = JsonConvert.DeserializeObject<CommServiceConfig>(param);
                return config;
            }
            catch (JsonException e)
            {
                Console.WriteLine($"JSONのパース中にエラーが発生しました: {e.Message}");
                return null;
            }
        }
        public EnvironmentService(string service_type, string file_type="local")
        {
            if (file_type == "local") {
                file_loader = new LocalFileLoader();
            }
            else {
                //TODO for unity
            }
            if (service_type == "dummy")
            {
                comm_service = new DummyCommunicationService();
            }
            else if (service_type == "udp")
            {
                var config = loadCommServiceConfig();
                comm_service = new UDPCommunicationService(config.Udp.LocalPort, config.Udp.RemoteIPAddress, config.Udp.RemotePort);
            }
            else if (service_type == "websocket_dotnet_localfile")
            {
                var config = loadCommServiceConfig();
                string serverUri = config.WebSocket.ServerURI;
                comm_service = new WebSocketCommunicationService(serverUri);
            }
            else if (service_type == "websocket_jslib")
            {
                //TODO webgl impl
            }


        }

        public ICommunicationService GetCommunication()
        {
            return comm_service;
        }

        public IFileLoader GetFileLoader()
        {
            return file_loader;
        }
    }

}
