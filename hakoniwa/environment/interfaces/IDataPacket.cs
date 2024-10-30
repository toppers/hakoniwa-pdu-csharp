using System;
namespace hakoniwa.environment.interfaces
{
    public interface IDataPacket
    {
        public string GetRobotName();
        public int GetChannelId();
        public byte[] GetPduData();
    }
}
