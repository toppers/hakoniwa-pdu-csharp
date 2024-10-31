using System;
using hakoniwa.environment.impl.local;
using hakoniwa.environment.interfaces;

namespace hakoniwa.environment.impl
{
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
        public EnvironmentService(string service_type, string file_type="local")
        {
            if (service_type == "dummy")
            {
                comm_service = new DummyCommunicationService();
            }
            else if (service_type == "udp")
            {
                //TODO fix params
                comm_service = new UDPCommunicationService(54001, "127.0.0.1", 54002);
            }
            else if (service_type == "websocket_dotnet_localfile")
            {
                //TODO fix params
                string serverUri = "ws://localhost:8080/echo";
                comm_service = new WebSocketCommunicationService(serverUri);
            }
            else if (service_type == "websocket_jslib")
            {
                //TODO webgl impl
            }
            if (file_type == "local") {
                file_loader = new LocalFileLoader();
            }
            else {
                //TODO for unity
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
