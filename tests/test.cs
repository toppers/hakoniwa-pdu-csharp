using System;
using hakoniwa.environment.impl;
using hakoniwa.environment.interfaces;
using hakoniwa.pdu.core;

namespace hakoniwa.pdu.test
{
    using System;
    using hakoniwa.pdu.interfaces;

    class Assert
    {
        private static bool test_failed = false;
        public static void IsTrue(bool condition, string message = "")
        {
            if (condition)
            {
                Console.WriteLine($"Test Passed: {message}");
            }
            else
            {
                Console.WriteLine($"Test Failed: {message}");
                test_failed = true;
            }
        }

        public static void IsFalse(bool condition, string message = "")
        {
            if (!condition)
            {
                Console.WriteLine($"Test Passed: {message}");
            }
            else
            {
                Console.WriteLine($"Test Failed: {message}");
                test_failed = true;
            }
        }

        public static void AreEqual(object expected, object actual, string message = "")
        {
            if (expected.Equals(actual))
            {
                Console.WriteLine($"Test Passed: {message}");
            }
            else
            {
                Console.WriteLine($"Test Failed: {message} - Expected {expected}, but got {actual}");
                test_failed = true;
            }
        }
        public static bool IsTestFailed()
        {
            return test_failed;
        }
    }


    class Program
    {
        private static string test_dir = "/Users/tmori/project/oss/hakoniwa-pdu-csharp/tests/test_data";
        static void Test_Twist()
        {
            string robotName = "DroneTransporter";
            string pduName = "drone_pos";
            IEnvironmentService service = EnvironmentServiceFactory.Create();
            PduManager mgr = new PduManager(service, test_dir);
            mgr.StartService();
            Assert.IsTrue(mgr != null, "Create Twist: pdu manager creation");

            /*
             * Create Test.
             */
            IPdu pdu = mgr.CreatePdu(robotName, pduName);
            Assert.IsTrue(pdu != null, "Write Twist: pdu is created");

            double x_val = pdu.GetData<IPdu>("linear").GetData<double>("x");
            Assert.IsTrue(x_val == 0, "Create Twist: data linear.x is OK");

            double z_val = pdu.GetData<IPdu>("angular").GetData<double>("z");
            Assert.IsTrue(z_val == 0, "Create Twist: data angular.z is OK");

            /*
             * Write Test.
             */
            pdu.GetData<IPdu>("linear").SetData<double>("x", 1.0);
            pdu.GetData<IPdu>("angular").SetData<double>("z", -1.0);
            mgr.WritePdu(robotName, pdu);

            /*
             * Read Test.
             */
            IPdu rpdu = mgr.ReadPdu(robotName, pduName);
            Assert.IsTrue(rpdu != null, "Read Twist: pdu is created");
            double r_x_val = rpdu.GetData<IPdu>("linear").GetData<double>("x");
            Assert.IsTrue(r_x_val == 1.0, "Read Twist: data linear.x is OK");

            double r_z_val = rpdu.GetData<IPdu>("angular").GetData<double>("z");
            Assert.IsTrue(r_z_val == -1.0, "Read Twist: data angular.z is OK");

            mgr.StopService();
        }
        static void HakoCameraData_Test()
        {
            string robotName = "DroneTransporter";
            string pduName = "hako_camera_data";
            IEnvironmentService service = EnvironmentServiceFactory.Create();
            PduManager mgr = new PduManager(service, test_dir);
            mgr.StartService();
            Assert.IsTrue(mgr != null, "Create HakoCameraData: pdu manager creation");

            /*
             * Create Test.
             */
            IPdu pdu = mgr.CreatePdu(robotName, pduName);
            Assert.IsTrue(pdu != null, "Write HakoCameraData: pdu is created");

            IPdu image_pdu = pdu.GetData<IPdu>("image");
            Assert.IsTrue(image_pdu != null, "Create HakoCameraData: image_pdu is created");
            byte[] images = image_pdu.GetDataArray<byte>("data");
            Assert.IsTrue(images[0] == 0, "Write HakoCameraData: image is empty");

            /*
             * Write Test.
             */
            images = new byte[128];
            images[0] = (byte)'a';
            images[127] = (byte)'b';
            image_pdu.SetData<Byte>("data", images);
            var test_images = pdu.GetData<IPdu>("image").GetDataArray<Byte>("data");
            Assert.IsTrue(images[127] == 'b', "Write HakoCameraData: image is ok");
            mgr.WritePdu(robotName, pdu);


            /*
             * Read Test.
             */
            IPdu rpdu = mgr.ReadPdu(robotName, pduName);
            Assert.IsTrue(rpdu != null, "Read Twist: pdu is created");
            byte[] rimages = rpdu.GetData<IPdu>("image").GetDataArray<Byte>("data");
            Assert.IsTrue(rimages[0] == 'a', "Read HakoCameraData: images[0] is OK");
            Assert.IsTrue(rimages[127] == 'b', "Read HakoCameraData: images[127] is OK");


            mgr.StopService();
        }
        static int Main()
        {
            Test_Twist();
            if (Assert.IsTestFailed())
            {
                Console.WriteLine("Test Failed.");
                return 1;
            }
            HakoCameraData_Test();
            if (Assert.IsTestFailed())
            {
                Console.WriteLine("Test Failed.");
                return 1;
            }

            Console.WriteLine("Test Success!");
            return 0;
        }
    }
}
