using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using hakoniwa.pdu.interfaces;

namespace hakoniwa.pdu.msgs.ev3_msgs
{
    public class Ev3PduSensorHeader
    {
        protected internal readonly IPdu _pdu;

        public Ev3PduSensorHeader(IPdu pdu)
        {
            _pdu = pdu;
        }

        public uint Timestamp
        {
            get => _pdu.GetData<uint>("timestamp");
            set => _pdu.SetData("timestamp", value);
        }        
    }
}