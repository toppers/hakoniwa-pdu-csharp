using System;
using System.Threading.Tasks;

namespace hakoniwa.environment.interfaces
{
    public interface ICommunicationBuffer
    {
        void PutPacket(IDataPacket packet);
    }
    public interface ICommunicationService
    {
        Task<bool> StartService(ICommunicationBuffer comm_buffer);
        bool StopService();
        bool IsServiceEnabled();
        Task<bool> SendData(string robotName, int channelId, byte[] pdu_data);
    }
}
