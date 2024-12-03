using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using hakoniwa.pdu.interfaces;

namespace hakoniwa.pdu.msgs.ev3_msgs
{
    public class Ev3PduColorSensor
    {
        protected internal readonly IPdu _pdu;

        public Ev3PduColorSensor(IPdu pdu)
        {
            _pdu = pdu;
        }

        public uint Color
        {
            get => _pdu.GetData<uint>("color");
            set => _pdu.SetData("color", value);
        }

        public uint Reflect
        {
            get => _pdu.GetData<uint>("reflect");
            set => _pdu.SetData("reflect", value);
        }

        public uint RgbR
        {
            get => _pdu.GetData<uint>("rgb_r");
            set => _pdu.SetData("rgb_r", value);
        }

        public uint RgbG
        {
            get => _pdu.GetData<uint>("rgb_g");
            set => _pdu.SetData("rgb_g", value);
        }

        public uint RgbB
        {
            get => _pdu.GetData<uint>("rgb_b");
            set => _pdu.SetData("rgb_b", value);
        }
    }
}