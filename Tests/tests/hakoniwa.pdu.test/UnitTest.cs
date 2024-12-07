namespace hakoniwa.pdu.test;
using System;
using Xunit;
using hakoniwa.environment.impl;
using hakoniwa.environment.interfaces;
using hakoniwa.pdu.interfaces;
using hakoniwa.pdu.core;
using hakoniwa.pdu.msgs.geometry_msgs;
using hakoniwa.pdu.msgs.hako_msgs;
using hakoniwa.pdu.msgs.sensor_msgs;
using hakoniwa.pdu.msgs.ev3_msgs;

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

        Twist twist = new Twist(pdu);

        //double x_val = pdu.GetData<IPdu>("linear").GetData<double>("x");
        double x_val = twist.linear.x;
        Assert.Equal(0, x_val);

        //double z_val = pdu.GetData<IPdu>("angular").GetData<double>("z");
        double z_val = twist.angular.z;
        Assert.Equal(0, z_val);

        /*
         * Write Test.
         */
        //pdu.GetData<IPdu>("linear").SetData<double>("x", 1.0);
        twist.linear.x = 1.0;
        //pdu.GetData<IPdu>("angular").SetData<double>("z", -1.0);
        twist.angular.z = -1.0;
        mgr.WritePdu(robotName, pdu);

        /*
         * Read Test.
         */
        IPdu rpdu = mgr.ReadPdu(robotName, pduName);
        Assert.NotNull(rpdu);

        Twist rtwist = new Twist(rpdu);

        //double r_x_val = rpdu.GetData<IPdu>("linear").GetData<double>("x");
        double r_x_val = rtwist.linear.x;
        Assert.Equal(1.0, r_x_val);

        //double r_z_val = rpdu.GetData<IPdu>("angular").GetData<double>("z");
        double r_z_val = rtwist.angular.z;
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

        HakoCameraData camera = new HakoCameraData(pdu);

        //IPdu image_pdu = pdu.GetData<IPdu>("image");
        //Assert.NotNull(image_pdu);

        byte[] images = camera.image.data;
        //byte[] images = image_pdu.GetDataArray<byte>("data");
        Assert.Equal(0, images[0]);

        /*
         * Write Test.
         */
        images = new byte[128];
        images[0] = (byte)'a';
        images[127] = (byte)'b';
        //image_pdu.SetData<byte>("data", images);
        camera.image.data = images;

        //byte[] test_images = pdu.GetData<IPdu>("image").GetDataArray<byte>("data");
        byte[] test_images = camera.image.data;
        Assert.Equal((byte)'a', test_images[0]);
        Assert.Equal((byte)'b', test_images[127]);
        mgr.WritePdu(robotName, pdu);

        /*
         * Read Test.
         */
        IPdu rpdu = mgr.ReadPdu(robotName, pduName);
        Assert.NotNull(rpdu);
        HakoCameraData rcamera = new HakoCameraData(rpdu);

        //byte[] rimages = rpdu.GetData<IPdu>("image").GetDataArray<byte>("data");
        byte[] rimages = rcamera.image.data;
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

        PointCloud2 pointCloud = new PointCloud2(pdu);

        // 初期のFieldsの確認
        Assert.Empty(pointCloud.fields);

        /*
         * Write Test.
         */
        PointField[] pointFields = new PointField[2];
        for (int i = 0; i < pointFields.Length; i++)
        {
            PointField field = new PointField(mgr.CreatePduByType("fields", "sensor_msgs", "PointField"));
            field.offset = (uint)(i + 100);
            pointFields[i] = field;
        }
        pointCloud.fields = pointFields;

        Assert.Equal(2, pointCloud.fields.Length);

        mgr.WritePdu(robotName, pdu);

        /*
         * Read Test.
         */
        IPdu rpdu = mgr.ReadPdu(robotName, pduName);
        PointCloud2 readPointCloud = new PointCloud2(rpdu);

        Assert.Equal(100u, readPointCloud.fields[0].offset);
        Assert.Equal(101u, readPointCloud.fields[1].offset);

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
        INamedPdu npdu = mgr.CreateNamedPdu(robotName, pduName);
        Assert.NotNull(npdu);

        Ev3PduSensor sensor = new Ev3PduSensor(npdu.Pdu);

        Assert.Equal(2, sensor.color_sensors.Length);
        Assert.Equal(2, sensor.touch_sensors.Length);
        Assert.Equal(3, sensor.motor_angle.Length);

        /*
         * Write Test.
         */
        sensor.color_sensors[0].rgb_r = 99;
        sensor.color_sensors[1].rgb_r = 101;
        sensor.touch_sensors[0].value = 9;
        sensor.touch_sensors[1].value = 8;
        sensor.motor_angle = new uint[] { 1, 2, 3 };

        Assert.Equal(99u, sensor.color_sensors[0].rgb_r);
        Assert.Equal(101u, sensor.color_sensors[1].rgb_r);
        Assert.Equal(9u, sensor.touch_sensors[0].value);
        Assert.Equal(8u, sensor.touch_sensors[1].value);
        Assert.Equal(1u, sensor.motor_angle[0]);
        Assert.Equal(2u, sensor.motor_angle[1]);
        Assert.Equal(3u, sensor.motor_angle[2]);

        mgr.WriteNamedPdu(npdu);

        /*
         * Read Test.
         */
        IPdu rpdu = mgr.ReadPdu(robotName, pduName);
        Ev3PduSensor readSensor = new Ev3PduSensor(rpdu);

        Assert.Equal(99u, readSensor.color_sensors[0].rgb_r);
        Assert.Equal(101u, readSensor.color_sensors[1].rgb_r);
        Assert.Equal(9u, readSensor.touch_sensors[0].value);
        Assert.Equal(8u, readSensor.touch_sensors[1].value);
        Assert.Equal(1u, readSensor.motor_angle[0]);
        Assert.Equal(2u, readSensor.motor_angle[1]);
        Assert.Equal(3u, readSensor.motor_angle[2]);

        mgr.StopService();
    }
}