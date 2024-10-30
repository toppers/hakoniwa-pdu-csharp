using System;
namespace hakoniwa.environment.interfaces
{
    public interface ICommunicationBuffer
    {
        void PutPacket(IDataPacket packet);
    }
    public interface ICommunicationService
    {
        void StartService(ICommunicationBuffer comm_buffer);
        void StopService();
        void SendData(IDataPacket packet);
    }
}
