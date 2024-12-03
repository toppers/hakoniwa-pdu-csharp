using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using hakoniwa.pdu.interfaces;

namespace hakoniwa.pdu.msgs.geometry_msgs
{
    public class Twist
    {
        protected internal readonly IPdu _pdu;

        public Twist(IPdu pdu)
        {
            _pdu = pdu;
        }

        // Linear field accessor
        private Vector3 _linear;
        public Vector3 Linear
        {
            get
            {
                if (_linear == null)
                {
                    _linear = new Vector3(_pdu.GetData<IPdu>("linear"));
                }
                return _linear;
            }
        }

        // Angular field accessor
        private Vector3 _angular;
        public Vector3 Angular
        {
            get
            {
                if (_angular == null)
                {
                    _angular = new Vector3(_pdu.GetData<IPdu>("angular"));
                }
                return _angular;
            }
        }
    }
}
