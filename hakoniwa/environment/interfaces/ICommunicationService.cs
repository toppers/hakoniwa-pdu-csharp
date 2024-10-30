using System;
namespace hakoniwa.environment.interfaces
{
    public interface ICommunicationService
    {
        void StartService();
        void StopService();
        void SendData(byte[] data);
        byte[] ReceiveData();
    }
}
