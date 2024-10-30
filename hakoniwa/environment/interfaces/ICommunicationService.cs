using System;
namespace hakoniwa.environment.interfaces
{
    public interface ICommunicationBuffer
    {
        void PutPacket(IDataPacket packet);
    }
    public interface ICommunicationService
    {
        bool StartService(ICommunicationBuffer comm_buffer);
        bool StopService();
        bool IsServiceEnabled();
        bool SendData(string robotName, int channelId, byte[] pdu_data);
    }
}
