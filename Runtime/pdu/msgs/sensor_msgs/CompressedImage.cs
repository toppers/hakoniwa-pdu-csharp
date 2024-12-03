using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using hakoniwa.pdu.interfaces;
using hakoniwa.pdu.msgs.std_msgs;

namespace hakoniwa.pdu.msgs.sensor_msgs
{
    public class CompressedImage
    {
        protected internal readonly IPdu _pdu;

        public CompressedImage(IPdu pdu)
        {
            _pdu = pdu;
        }

        private Header _header;
        public Header Header
        {
            get
            {
                if (_header == null)
                {
                    _header = new Header(_pdu.GetData<IPdu>("header"));
                }
                return _header;
            }
        }

        public string Format
        {
            get => _pdu.GetData<string>("format");
            set => _pdu.SetData("format", value);
        }

        public byte[] Data
        {
            get => _pdu.GetDataArray<byte>("data");
            set => _pdu.SetData("data", value);
        }
    }
}