using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace hakoniwa.pdu.interfaces
{
    public interface IPduManager
    {
        Task<bool> StartService(string server_uri = null);
        bool StopService();
        IPdu CreatePdu(string robotName, string pduName);
        IPdu CreatePduByType(string pduName, string packageName, string typeName);
        string WritePdu(string robotName, IPdu pdu);
        Task<bool> FlushPdu(string robotName, string pduName);
        IPdu ReadPdu(string robotName, string pduName);
        string GetKey(string robotName, string pduName);

    }
}