namespace hakoniwa.pdu.test;

using System;
using System.Text;
using Xunit;
using hakoniwa.environment.impl;

public class DataPacketTest
{
    [Fact]
    public void DataPacket_DefaultEncodeDecode_UsesV1Compatibility()
    {
        byte[] body = Encoding.UTF8.GetBytes("hello-v1");
        var packet = new DataPacket
        {
            RobotName = "DroneTransporter",
            ChannelId = 3,
            BodyData = body
        };

        byte[] encoded = packet.Encode();
        var decoded = DataPacket.Decode(encoded);

        Assert.NotNull(decoded);
        Assert.Equal("DroneTransporter", decoded.GetRobotName());
        Assert.Equal(3, decoded.GetChannelId());
        Assert.Equal(body, decoded.GetPduData());
    }

    [Fact]
    public void DataPacket_V2EncodeDecode_CanRoundTrip()
    {
        byte[] body = new byte[] { 1, 2, 3, 4, 5 };
        var packet = new DataPacket
        {
            RobotName = "Drone-1",
            ChannelId = 7,
            BodyData = body
        };

        byte[] encoded = packet.Encode("v2");
        var decoded = DataPacket.Decode(encoded, "v2");

        Assert.NotNull(decoded);
        Assert.Equal(304 + body.Length, encoded.Length);
        Assert.Equal("Drone-1", decoded.GetRobotName());
        Assert.Equal(7, decoded.GetChannelId());
        Assert.Equal(body, decoded.GetPduData());
        Assert.Equal((uint)0x48414B4F, BitConverter.ToUInt32(encoded, 128));
        Assert.Equal((ushort)2, BitConverter.ToUInt16(encoded, 132));
    }
}
