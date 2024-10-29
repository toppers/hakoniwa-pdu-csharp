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
            Assert.IsTrue(images[0] == 'a', "Write HakoCameraData: image[0] is a");
            Assert.IsTrue(images[127] == 'b', "Write HakoCameraData: image[127] is b");
            mgr.WritePdu(robotName, pdu);


            /*
             * Read Test.
             */
            IPdu rpdu = mgr.ReadPdu(robotName, pduName);
            Assert.IsTrue(rpdu != null, "Read HakoCameraData: pdu is read");
            byte[] rimages = rpdu.GetData<IPdu>("image").GetDataArray<Byte>("data");
            Assert.IsTrue(rimages.Length == 128, "Read HakoCameraData size = 128.");
            Assert.IsTrue(rimages[0] == 'a', "Read HakoCameraData: images[0] is a");
            Assert.IsTrue(rimages[127] == 'b', "Read HakoCameraData: images[127] is b");


            mgr.StopService();
        }
        static void PointCloud2_Test()
        {
            string robotName = "DroneTransporter";
            string pduName = "lidar_points";
            IEnvironmentService service = EnvironmentServiceFactory.Create();
            PduManager mgr = new PduManager(service, test_dir);
            mgr.StartService();
            Assert.IsTrue(mgr != null, "Create PointCloud2: pdu manager creation");

            /*
             * Create Test.
             */
            IPdu pdu = mgr.CreatePdu(robotName, pduName);
            Assert.IsTrue(pdu != null, "Write PointCloud2: pdu is created");
            IPdu[] r_pdu = pdu.GetDataArray<IPdu>("fields");
            Assert.IsTrue(r_pdu.Length == 0, "Write PointCloud2: pdu's fields is empty");


            /*
             * Write Test.
             */
            IPdu[] pdu_fields = new IPdu[2];
            for (int i = 0; i < pdu_fields.Length; i++)
            {
                pdu_fields[i] = mgr.CreatePduByType("fields", "sensor_msgs", "PointField");
                pdu_fields[i].SetData<UInt32>("offset", (UInt32)(i + 100));
            }
            pdu.SetData<IPdu>("fields", pdu_fields);
            var rets = pdu.GetDataArray<IPdu>("fields");
            Assert.IsTrue(rets.Length == 2, "Write PointCloud2: pdu's fields array size is 2");
            //Console.WriteLine($"off1: {pdu.GetDataArray<IPdu>("fields")[0].GetData<UInt32>("offset")}");
            //Console.WriteLine($"off2: {pdu.GetDataArray<IPdu>("fields")[1].GetData<UInt32>("offset")}");
            mgr.WritePdu(robotName, pdu);

            /*
             * Read Test.
             */
            IPdu wcheck_pdu = mgr.ReadPdu(robotName, pduName);
            Assert.IsTrue(wcheck_pdu.GetDataArray<IPdu>("fields")[0].GetData<UInt32>("offset") == 100, "Write PointCloud2: pdu write[0] test OK");
            Assert.IsTrue(wcheck_pdu.GetDataArray<IPdu>("fields")[1].GetData<UInt32>("offset") == 101, "Write PointCloud2: pdu write[1] test OK");
            //Console.WriteLine($"off1: {wcheck_pdu.GetDataArray<IPdu>("fields")[0].GetData<UInt32>("offset")}");
            //Console.WriteLine($"off2: {wcheck_pdu.GetDataArray<IPdu>("fields")[1].GetData<UInt32>("offset")}");
            mgr.StopService();
        }
        static void Ev3PduSensor_Test()
        {
            string robotName = "DroneTransporter";
            string pduName = "ev3_sensor";
            IEnvironmentService service = EnvironmentServiceFactory.Create();
            PduManager mgr = new PduManager(service, test_dir);
            mgr.StartService();
            Assert.IsTrue(mgr != null, "Create Ev3PduSensor: pdu manager creation");

            /*
             * Create Test.
             */
            IPdu pdu = mgr.CreatePdu(robotName, pduName);
            Assert.IsTrue(pdu != null, "Write Ev3PduSensor: pdu is created");
            IPdu[] r_pdu = pdu.GetDataArray<IPdu>("color_sensors");
            Assert.IsTrue(r_pdu.Length == 2, "Write Ev3PduSensor: pdu's color_sensors len = 2");
            r_pdu = pdu.GetDataArray<IPdu>("touch_sensors");
            Assert.IsTrue(r_pdu.Length == 2, "Write Ev3PduSensor: pdu's touch_sensors len = 2");
            var fixed_array = pdu.GetDataArray<UInt32>("motor_angle");
            Assert.IsTrue(fixed_array.Length == 3, "Write Ev3PduSensor: pdu's motor_angle len = 3");
            //Console.WriteLine($"motor angle len: {fixed_array.Length}");

            /*
             * Write Test.
             */
            pdu.GetDataArray<IPdu>("color_sensors")[0].SetData<UInt32>("rgb_r", 99);
            pdu.GetDataArray<IPdu>("color_sensors")[1].SetData<UInt32>("rgb_r", 101);
            pdu.GetDataArray<IPdu>("touch_sensors")[0].SetData<UInt32>("value", 9);
            pdu.GetDataArray<IPdu>("touch_sensors")[1].SetData<UInt32>("value", 8);
            pdu.SetData<UInt32>("motor_angle", 0, 1);
            pdu.SetData<UInt32>("motor_angle", 1, 2);
            pdu.SetData<UInt32>("motor_angle", 2, 3);
            Assert.IsTrue(pdu.GetDataArray<IPdu>("color_sensors")[0].GetData<UInt32>("rgb_r")== 99, "Read Ev3PduSensor: pdu's color_sensors 99");
            Assert.IsTrue(pdu.GetDataArray<IPdu>("color_sensors")[1].GetData<UInt32>("rgb_r") == 101, "Read Ev3PduSensor: pdu's color_sensors 101");
            Assert.IsTrue(pdu.GetDataArray<IPdu>("touch_sensors")[0].GetData<UInt32>("value") == 9, "Read Ev3PduSensor: pdu's touch_sensors 9");
            Assert.IsTrue(pdu.GetDataArray<IPdu>("touch_sensors")[1].GetData<UInt32>("value") == 8, "Read Ev3PduSensor: pdu's touch_sensors 8");
            Assert.IsTrue(pdu.GetDataArray<UInt32>("motor_angle")[0] == 1, "Read Ev3PduSensor: pdu's motor_angle 0");
            Assert.IsTrue(pdu.GetDataArray<UInt32>("motor_angle")[1] == 2, "Read Ev3PduSensor: pdu's motor_angle 1");
            Assert.IsTrue(pdu.GetDataArray<UInt32>("motor_angle")[2] == 3, "Read Ev3PduSensor: pdu's motor_angle 2");

            mgr.WritePdu(robotName, pdu);
            /*
             * Read Test.
             */
            IPdu wcheck_pdu = mgr.ReadPdu(robotName, pduName);
            Assert.IsTrue(wcheck_pdu.GetDataArray<IPdu>("color_sensors")[0].GetData<UInt32>("rgb_r") == 99, "Read Ev3PduSensor: pdu's color_sensors 99");
            Assert.IsTrue(wcheck_pdu.GetDataArray<IPdu>("color_sensors")[1].GetData<UInt32>("rgb_r") == 101, "Read Ev3PduSensor: pdu's color_sensors 101");
            Assert.IsTrue(wcheck_pdu.GetDataArray<IPdu>("touch_sensors")[0].GetData<UInt32>("value") == 9, "Read Ev3PduSensor: pdu's touch_sensors 9");
            Assert.IsTrue(wcheck_pdu.GetDataArray<IPdu>("touch_sensors")[1].GetData<UInt32>("value") == 8, "Read Ev3PduSensor: pdu's touch_sensors 8");
            Assert.IsTrue(wcheck_pdu.GetDataArray<UInt32>("motor_angle")[0] == 1, "Read Ev3PduSensor: pdu's motor_angle 0");
            Assert.IsTrue(wcheck_pdu.GetDataArray<UInt32>("motor_angle")[1] == 2, "Read Ev3PduSensor: pdu's motor_angle 1");
            Assert.IsTrue(wcheck_pdu.GetDataArray<UInt32>("motor_angle")[2] == 3, "Read Ev3PduSensor: pdu's motor_angle 2");

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
            PointCloud2_Test();
            if (Assert.IsTestFailed())
            {
                Console.WriteLine("Test Failed.");
                return 1;
            }
            Ev3PduSensor_Test();
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
