using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using hakoniwa.pdu.interfaces;
using hakoniwa.pdu.msgs.builtin_interfaces;

namespace hakoniwa.pdu.msgs.std_msgs
{
    public class Header
    {
        protected internal readonly IPdu _pdu;

        public Header(IPdu pdu)
        {
            _pdu = pdu;
        }

        public Time Stamp => new Time(_pdu.GetData<IPdu>("stamp"));

        public string FrameId
        {
            get => _pdu.GetData<string>("frame_id");
            set => _pdu.SetData("frame_id", value);
        }
    }
}
