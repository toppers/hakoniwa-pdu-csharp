using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using hakoniwa.pdu.interfaces;
using hakoniwa.pdu.msgs.sensor_msgs;

namespace hakoniwa.pdu.msgs.hako_msgs
{
    public class HakoCameraData
    {
        protected internal readonly IPdu _pdu;

        public HakoCameraData(IPdu pdu)
        {
            _pdu = pdu;
        }

        public int RequestId
        {
            get => _pdu.GetData<int>("request_id");
            set => _pdu.SetData("request_id", value);
        }

        private CompressedImage _image;
        public CompressedImage Image 
        {
            get 
            {
                if (_image == null)
                {
                    _image = new CompressedImage(_pdu.GetData<IPdu>("image"));
                }
                return _image;
            }
        }
    }
}