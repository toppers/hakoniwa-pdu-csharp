using System;
using System.Collections.Generic;
using hakoniwa.environment.interfaces;

namespace hakoniwa.pdu.core
{
    public class PduBuffer: ICommunicationBuffer
    {
        private readonly Dictionary<string, byte[]> pduBuffer;
        private readonly object lockObj = new object();
        private PduChannelConfig channel_config;

        public PduBuffer(PduChannelConfig pdu_channel_config)
        {
            pduBuffer = new Dictionary<string, byte[]>();
            channel_config = pdu_channel_config;
        }

        public void SetBuffer(string key, byte[] data)
        {
            lock (lockObj)
            {
                pduBuffer[key] = data;
            }
        }

        public byte[] GetBuffer(string key)
        {
            lock (lockObj)
            {
                if (pduBuffer.TryGetValue(key, out var data))
                {
                    pduBuffer.Remove(key); // キーとデータを削除
                    return data;
                }
                return null;
            }
        }

        public bool ContainsBuffer(string key)
        {
            lock (lockObj)
            {
                return pduBuffer.ContainsKey(key);
            }
        }

        public void Clear()
        {
            lock (lockObj)
            {
                pduBuffer.Clear();
            }
        }

        public void PutPacket(IDataPacket packet)
        {
            string pduName = this.channel_config.GetPduName(packet.GetRobotName(), packet.GetChannelId());
            if (pduName != null)
            {
                string key = packet.GetRobotName() + "_" + pduName;
                Console.WriteLine($"put packet: {key}");
                this.SetBuffer(key, packet.GetPduData());
            }
        }
    }
}
