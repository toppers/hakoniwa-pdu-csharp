using System;
using hakoniwa.environment.impl.local;
#if !NO_USE_UNITY
using hakoniwa.environment.impl.unity;
#endif
using hakoniwa.environment.interfaces;
using Newtonsoft.Json;
using System.IO;

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
        static public IEnvironmentService Create(string service_type, string file_type="local", string path=".")
        {
            return new EnvironmentService(service_type, file_type, path);
        }
    }
    public class EnvironmentService : IEnvironmentService
    {
        IFileLoader file_loader;
        ICommunicationService comm_service;

        public void SetCommunication(ICommunicationService comm)
        {
            this.comm_service = comm;
        }

        private CommServiceConfig loadCommServiceConfig(string path)
        {
            string relative_path = path + "/" + "comm_service_config";
            string normalizedPath = relative_path.Replace('/', Path.DirectorySeparatorChar);
            string param = file_loader.LoadText(normalizedPath, ".json");
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
        public EnvironmentService(string service_type, string file_type, string path)
        {
            if (file_type == "local") {
                file_loader = new LocalFileLoader();
            }
#if !NO_USE_UNITY
            else if (file_type == "unity") {
                file_loader = new ResourcesFileLoader();
            }
#endif
            if (service_type == "dummy")
            {
                comm_service = new DummyCommunicationService();
            }
            else if (service_type == "udp")
            {
                var config = loadCommServiceConfig(path);
                comm_service = new UDPCommunicationService(config.Udp.LocalPort, config.Udp.RemoteIPAddress, config.Udp.RemotePort);
            }
            else if (service_type == "websocket_dotnet")
            {
                var config = loadCommServiceConfig(path);
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
