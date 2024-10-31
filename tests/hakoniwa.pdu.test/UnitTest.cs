namespace hakoniwa.pdu.test;
using System;
using Xunit;
using hakoniwa.environment.impl;
using hakoniwa.environment.interfaces;
using hakoniwa.pdu.interfaces;
using hakoniwa.pdu.core;

public class UnitTest
{
    private static readonly string testDir = "test_data";

    [Fact]
    public void Test_Twist()
    {
        string robotName = "DroneTransporter";
        string pduName = "drone_pos";
        IEnvironmentService service = EnvironmentServiceFactory.Create("dummy");
        PduManager mgr = new PduManager(service, testDir);
        mgr.StartService();

        // PduManagerの作成テスト
        Assert.NotNull(mgr);

        /*
         * Create Test.
         */
        IPdu pdu = mgr.CreatePdu(robotName, pduName);
        Assert.NotNull(pdu);

        double x_val = pdu.GetData<IPdu>("linear").GetData<double>("x");
        Assert.Equal(0, x_val);

        double z_val = pdu.GetData<IPdu>("angular").GetData<double>("z");
        Assert.Equal(0, z_val);

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
        Assert.NotNull(rpdu);

        double r_x_val = rpdu.GetData<IPdu>("linear").GetData<double>("x");
        Assert.Equal(1.0, r_x_val);

        double r_z_val = rpdu.GetData<IPdu>("angular").GetData<double>("z");
        Assert.Equal(-1.0, r_z_val);

        mgr.StopService();
    }

    [Fact]
    public void HakoCameraData_Test()
    {
        string robotName = "DroneTransporter";
        string pduName = "hako_camera_data";
        IEnvironmentService service = EnvironmentServiceFactory.Create("dummy");
        PduManager mgr = new PduManager(service, testDir);
        mgr.StartService();

        // PduManagerの作成テスト
        Assert.NotNull(mgr);

        /*
         * Create Test.
         */
        IPdu pdu = mgr.CreatePdu(robotName, pduName);
        Assert.NotNull(pdu);

        IPdu image_pdu = pdu.GetData<IPdu>("image");
        Assert.NotNull(image_pdu);

        byte[] images = image_pdu.GetDataArray<byte>("data");
        Assert.Equal(0, images[0]);

        /*
         * Write Test.
         */
        images = new byte[128];
        images[0] = (byte)'a';
        images[127] = (byte)'b';
        image_pdu.SetData<byte>("data", images);

        byte[] test_images = pdu.GetData<IPdu>("image").GetDataArray<byte>("data");
        Assert.Equal((byte)'a', test_images[0]);
        Assert.Equal((byte)'b', test_images[127]);
        mgr.WritePdu(robotName, pdu);

        /*
         * Read Test.
         */
        IPdu rpdu = mgr.ReadPdu(robotName, pduName);
        Assert.NotNull(rpdu);

        byte[] rimages = rpdu.GetData<IPdu>("image").GetDataArray<byte>("data");
        Assert.Equal(128, rimages.Length);
        Assert.Equal((byte)'a', rimages[0]);
        Assert.Equal((byte)'b', rimages[127]);

        mgr.StopService();
    }

    [Fact]
    public void PointCloud2_Test()
    {
        string robotName = "DroneTransporter";
        string pduName = "lidar_points";
        IEnvironmentService service = EnvironmentServiceFactory.Create("dummy");
        PduManager mgr = new PduManager(service, testDir);
        mgr.StartService();

        // PduManagerの作成テスト
        Assert.NotNull(mgr);

        /*
         * Create Test.
         */
        IPdu pdu = mgr.CreatePdu(robotName, pduName);
        Assert.NotNull(pdu);

        IPdu[] r_pdu = pdu.GetDataArray<IPdu>("fields");
        Assert.Empty(r_pdu);

        /*
         * Write Test.
         */
        IPdu[] pdu_fields = new IPdu[2];
        for (int i = 0; i < pdu_fields.Length; i++)
        {
            pdu_fields[i] = mgr.CreatePduByType("fields", "sensor_msgs", "PointField");
            pdu_fields[i].SetData<uint>("offset", (uint)(i + 100));
        }
        pdu.SetData<IPdu>("fields", pdu_fields);

        var rets = pdu.GetDataArray<IPdu>("fields");
        Assert.Equal(2, rets.Length);

        mgr.WritePdu(robotName, pdu);

        /*
         * Read Test.
         */
        IPdu wcheck_pdu = mgr.ReadPdu(robotName, pduName);
        Assert.Equal(100u, wcheck_pdu.GetDataArray<IPdu>("fields")[0].GetData<uint>("offset"));
        Assert.Equal(101u, wcheck_pdu.GetDataArray<IPdu>("fields")[1].GetData<uint>("offset"));

        mgr.StopService();
    }

    [Fact]
    public void Ev3PduSensor_Test()
    {
        string robotName = "DroneTransporter";
        string pduName = "ev3_sensor";
        IEnvironmentService service = EnvironmentServiceFactory.Create("dummy");
        PduManager mgr = new PduManager(service, testDir);
        mgr.StartService();

        // PduManagerの作成テスト
        Assert.NotNull(mgr);

        /*
         * Create Test.
         */
        IPdu pdu = mgr.CreatePdu(robotName, pduName);
        Assert.NotNull(pdu);

        IPdu[] colorSensors = pdu.GetDataArray<IPdu>("color_sensors");
        Assert.Equal(2, colorSensors.Length);

        IPdu[] touchSensors = pdu.GetDataArray<IPdu>("touch_sensors");
        Assert.Equal(2, touchSensors.Length);

        var motorAngles = pdu.GetDataArray<uint>("motor_angle");
        Assert.Equal(3, motorAngles.Length);

        /*
         * Write Test.
         */
        pdu.GetDataArray<IPdu>("color_sensors")[0].SetData<uint>("rgb_r", 99);
        pdu.GetDataArray<IPdu>("color_sensors")[1].SetData<uint>("rgb_r", 101);
        pdu.GetDataArray<IPdu>("touch_sensors")[0].SetData<uint>("value", 9);
        pdu.GetDataArray<IPdu>("touch_sensors")[1].SetData<uint>("value", 8);
        pdu.SetData<uint>("motor_angle", 0, 1);
        pdu.SetData<uint>("motor_angle", 1, 2);
        pdu.SetData<uint>("motor_angle", 2, 3);

        Assert.Equal(99u, pdu.GetDataArray<IPdu>("color_sensors")[0].GetData<uint>("rgb_r"));
        Assert.Equal(101u, pdu.GetDataArray<IPdu>("color_sensors")[1].GetData<uint>("rgb_r"));
        Assert.Equal(9u, pdu.GetDataArray<IPdu>("touch_sensors")[0].GetData<uint>("value"));
        Assert.Equal(8u, pdu.GetDataArray<IPdu>("touch_sensors")[1].GetData<uint>("value"));
        Assert.Equal(1u, pdu.GetDataArray<uint>("motor_angle")[0]);
        Assert.Equal(2u, pdu.GetDataArray<uint>("motor_angle")[1]);
        Assert.Equal(3u, pdu.GetDataArray<uint>("motor_angle")[2]);

        mgr.WritePdu(robotName, pdu);

        /*
         * Read Test.
         */
        IPdu wcheck_pdu = mgr.ReadPdu(robotName, pduName);
        Assert.Equal(99u, wcheck_pdu.GetDataArray<IPdu>("color_sensors")[0].GetData<uint>("rgb_r"));
        Assert.Equal(101u, wcheck_pdu.GetDataArray<IPdu>("color_sensors")[1].GetData<uint>("rgb_r"));
        Assert.Equal(9u, wcheck_pdu.GetDataArray<IPdu>("touch_sensors")[0].GetData<uint>("value"));
        Assert.Equal(8u, wcheck_pdu.GetDataArray<IPdu>("touch_sensors")[1].GetData<uint>("value"));
        Assert.Equal(1u, wcheck_pdu.GetDataArray<uint>("motor_angle")[0]);
        Assert.Equal(2u, wcheck_pdu.GetDataArray<uint>("motor_angle")[1]);
        Assert.Equal(3u, wcheck_pdu.GetDataArray<uint>("motor_angle")[2]);

        mgr.StopService();
    }
}