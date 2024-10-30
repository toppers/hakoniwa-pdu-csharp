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
        public EnvironmentService(string service_type)
        {
            file_loader = new LocalFileLoader();
            if (service_type == "dummy")
            {
                comm_service = new DummyCommunicationService();
            }
            else if (service_type == "udp")
            {
                //TODO fix params
                comm_service = new UDPCommunicationService(54001, "127.0.0.1", 54002);
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
