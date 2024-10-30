using System;
using hakoniwa.environment.interfaces;

namespace hakoniwa.environment.impl.local
{
    public class DummyCommunicationService: ICommunicationService
    {
        private bool isServiceEnabled = false;
        public DummyCommunicationService()
        {
        }

        public bool IsServiceEnabled()
        {
            return isServiceEnabled;
        }

        public bool SendData(string robotName, int channelId, byte[] pdu_data)
        {
            if (!isServiceEnabled)
            {
                return false;
            }
            return true;
        }

        public bool StartService(ICommunicationBuffer comm_buffer)
        {
            if (isServiceEnabled)
            {
                return false;
            }
            isServiceEnabled = true;
            return true;
        }

        public bool StopService()
        {
            if (isServiceEnabled)
            {
                return false;
            }
            return true;
        }
    }
}
