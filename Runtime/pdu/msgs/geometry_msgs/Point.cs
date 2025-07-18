using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using hakoniwa.pdu.interfaces;

namespace hakoniwa.pdu.msgs.geometry_msgs
{
    public class Point
    {
        protected internal readonly IPdu _pdu;
        public IPdu GetPdu() { return _pdu; }

        public Point(IPdu pdu)
        {
            _pdu = pdu;
        }
        public double x
        {
            get => _pdu.GetData<double>("x");
            set => _pdu.SetData("x", value);
        }
        public double y
        {
            get => _pdu.GetData<double>("y");
            set => _pdu.SetData("y", value);
        }
        public double z
        {
            get => _pdu.GetData<double>("z");
            set => _pdu.SetData("z", value);
        }
    }
}
