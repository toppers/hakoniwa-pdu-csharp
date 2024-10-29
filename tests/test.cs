using System;
using hakoniwa.environment.impl;
using hakoniwa.environment.interfaces;
using hakoniwa.pdu.core;

namespace hakoniwa.pdu.test
{
    class Program
    {
        static int Main()
        {
            IEnvironmentService service = EnvironmentServiceFactory.Create();
            PduManager mangaer = new PduManager(service, "/Users/tmori/project/oss/hakoniwa-pdu-charp/tests/test_data");
            Console.WriteLine("OK");
            return 0;
        }
    }
}
