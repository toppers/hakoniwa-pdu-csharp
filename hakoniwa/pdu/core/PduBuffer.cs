using System;
using System.Collections.Generic;

namespace hakoniwa.pdu.core
{
    public class PduBuffer
    {
        private readonly Dictionary<string, byte[]> pduBuffer;
        private readonly object lockObj = new object();

        public PduBuffer()
        {
            pduBuffer = new Dictionary<string, byte[]>();
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
    }
}
