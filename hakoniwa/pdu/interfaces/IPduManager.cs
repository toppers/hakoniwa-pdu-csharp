using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace hakoniwa.pdu.interfaces
{
    public interface IPduManager
    {
        bool StartService();
        bool StopService();
        IPdu CreatePdu(string robotName, string pduName);
        IPdu CreatePduByType(string pduName, string packageName, string typeName);
        string WritePdu(string robotName, IPdu pdu);
        bool FlushPdu(string key);
        IPdu ReadPdu(string robotName, string pduName);
        string GetKey(string robotName, string pduName);

    }
}