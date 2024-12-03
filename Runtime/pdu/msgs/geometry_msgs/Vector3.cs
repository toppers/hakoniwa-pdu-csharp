using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using hakoniwa.pdu.interfaces;

namespace hakoniwa.pdu.msgs.geometry_msgs
{
    public class Vector3
    {
        protected internal readonly IPdu _pdu;

        public Vector3(IPdu pdu)
        {
            _pdu = pdu;
        }

        public double X
        {
            get => _pdu.GetData<double>("x");
            set => _pdu.SetData("x", value);
        }

        public double Y
        {
            get => _pdu.GetData<double>("y");
            set => _pdu.SetData("y", value);
        }

        public double Z
        {
            get => _pdu.GetData<double>("z");
            set => _pdu.SetData("z", value);
        }
    }
}