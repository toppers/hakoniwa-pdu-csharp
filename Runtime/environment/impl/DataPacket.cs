using System;
using System.Text;
using hakoniwa.environment.interfaces;

namespace hakoniwa.environment.impl
{
    public class DataPacket: IDataPacket
    {
        private const uint HakoMetaMagic = 0x48414B4F;
        private const ushort HakoMetaVersionV2 = 0x0002;
        private const uint PduDataType = 0x42555043;
        private const int RobotNameFieldSize = 128;
        private const int MetaV2FixedSize = 176;
        private const int TotalMetaV2Size = RobotNameFieldSize + MetaV2FixedSize;

        public string RobotName { get; set; }
        public int ChannelId { get; set; }
        public byte[] BodyData { get; set; }
        public int GetChannelId()
        {
            return ChannelId;
        }

        public byte[] GetPduData()
        {
            return BodyData;
        }

        public string GetRobotName()
        {
            return RobotName;
        }

        /// <summary>
        /// データパケットの構造
        /// - 先頭4バイト: 後続するデータの全体長 (int32)
        /// - 次の4バイト: ヘッダデータの長さ (int32)
        /// - ヘッダデータ構造:
        ///     1. ロボット名の長さ (uint32)
        ///     2. ロボット名 (可変長の文字列、UTF-8エンコード)
        ///     3. チャネルID (int32)
        /// - ボディ部: 残りのデータはプロトコルでの解釈は行わず、バイナリデータとして保持
        /// </summary>
        /// <param name="data">解析するデータパケット</param>
        /// <returns>解析結果を保持する ParsedData オブジェクト</returns>
        /// <exception cref="ArgumentException">データの長さが不足している場合</exception>
        public static IDataPacket Decode(byte[] data, string version = "v1")
        {
            if (version == "v2")
            {
                return DecodeV2(data);
            }
            return DecodeV1(data);
        }

        private static IDataPacket DecodeV1(byte[] data)
        {
            if (data.Length < 12)
                throw new ArgumentException("データが短すぎます。");

            int currentIndex = 0;

            // ヘッダーの長さを取得
            int headerLength = BitConverter.ToInt32(data, currentIndex);
            currentIndex += 4;

            if (data.Length < 4 + headerLength)
                throw new ArgumentException("指定されたヘッダー長が不正です。");

            // ロボット名の長さを取得
            int robotNameLength = BitConverter.ToInt32(data, currentIndex);
            currentIndex += 4;

            // ロボット名を取得
            string robotName = Encoding.UTF8.GetString(data, currentIndex, robotNameLength);
            currentIndex += robotNameLength;

            // チャネルIDを取得
            int channelId = BitConverter.ToInt32(data, currentIndex);
            currentIndex += 4;

            // ボディデータの開始位置と長さを計算
            int bodyLength = data.Length - currentIndex;
            byte[] bodyData = new byte[bodyLength];
            Array.Copy(data, currentIndex, bodyData, 0, bodyLength);

            return new DataPacket
            {
                RobotName = robotName,
                ChannelId = channelId,
                BodyData = bodyData
            };
        }

        private static IDataPacket DecodeV2(byte[] data)
        {
            if (data.Length < TotalMetaV2Size)
                throw new ArgumentException("データが短すぎます。");

            string robotName = ReadNullTerminatedString(data, 0, RobotNameFieldSize);
            uint magic = BitConverter.ToUInt32(data, RobotNameFieldSize);
            ushort version = BitConverter.ToUInt16(data, RobotNameFieldSize + 4);
            uint metaRequestType = BitConverter.ToUInt32(data, RobotNameFieldSize + 12);
            uint bodyLength = BitConverter.ToUInt32(data, RobotNameFieldSize + 20);
            int channelId = BitConverter.ToInt32(data, RobotNameFieldSize + 48);

            if (magic != HakoMetaMagic)
                throw new ArgumentException("magic number が不正です。");
            if (version != HakoMetaVersionV2)
                throw new ArgumentException("packet version が不正です。");
            if (metaRequestType != PduDataType)
                throw new ArgumentException("meta request type が不正です。");
            if (data.Length < TotalMetaV2Size + bodyLength)
                throw new ArgumentException("body length が不正です。");

            byte[] bodyData = new byte[bodyLength];
            Array.Copy(data, TotalMetaV2Size, bodyData, 0, bodyLength);

            return new DataPacket
            {
                RobotName = robotName,
                ChannelId = channelId,
                BodyData = bodyData
            };
        }


        /// <summary>
        /// DataPacket オブジェクトからバイト配列にエンコードします。
        /// </summary>
        public byte[] Encode()
        {
            return Encode("v1");
        }

        public byte[] Encode(string version)
        {
            if (version == "v2")
            {
                return EncodeV2();
            }
            return EncodeV1();
        }

        private byte[] EncodeV1()
        {
            byte[] robotNameBytes = Encoding.UTF8.GetBytes(GetRobotName());
            int robotNameLength = robotNameBytes.Length;

            int headerLength = 
                4                                   // robot name length
                + robotNameLength                   // robot name
                + 4                                 // channel_id
                + (GetPduData()?.Length ?? 0);      //body length
            int totalLength = 4 + headerLength;

            byte[] data = new byte[totalLength];
            int currentIndex = 0;

            //header length
            BitConverter.GetBytes(headerLength).CopyTo(data, currentIndex);
            currentIndex += 4;
            //header
            {
                // robot name length
                BitConverter.GetBytes(robotNameLength).CopyTo(data, currentIndex);
                currentIndex += 4;

                // robot name
                robotNameBytes.CopyTo(data, currentIndex);
                currentIndex += robotNameLength;

                // channel id
                BitConverter.GetBytes(GetChannelId()).CopyTo(data, currentIndex);
                currentIndex += 4;
            }
            // body
            {
                GetPduData()?.CopyTo(data, currentIndex);
            }

            return data;
        }

        private byte[] EncodeV2()
        {
            byte[] body = GetPduData() ?? Array.Empty<byte>();
            byte[] data = new byte[TotalMetaV2Size + body.Length];

            WriteFixedString(data, 0, RobotNameFieldSize, GetRobotName());
            BitConverter.GetBytes(HakoMetaMagic).CopyTo(data, RobotNameFieldSize);
            BitConverter.GetBytes(HakoMetaVersionV2).CopyTo(data, RobotNameFieldSize + 4);
            BitConverter.GetBytes((ushort)0).CopyTo(data, RobotNameFieldSize + 6);
            BitConverter.GetBytes(0u).CopyTo(data, RobotNameFieldSize + 8);
            BitConverter.GetBytes(PduDataType).CopyTo(data, RobotNameFieldSize + 12);
            BitConverter.GetBytes((uint)(MetaV2FixedSize - 4 + body.Length)).CopyTo(data, RobotNameFieldSize + 16);
            BitConverter.GetBytes((uint)body.Length).CopyTo(data, RobotNameFieldSize + 20);
            BitConverter.GetBytes(0L).CopyTo(data, RobotNameFieldSize + 24);
            BitConverter.GetBytes(0L).CopyTo(data, RobotNameFieldSize + 32);
            BitConverter.GetBytes(0L).CopyTo(data, RobotNameFieldSize + 40);
            BitConverter.GetBytes(GetChannelId()).CopyTo(data, RobotNameFieldSize + 48);

            body.CopyTo(data, TotalMetaV2Size);
            return data;
        }

        private static void WriteFixedString(byte[] destination, int offset, int fieldSize, string value)
        {
            Array.Clear(destination, offset, fieldSize);
            byte[] bytes = Encoding.UTF8.GetBytes(value ?? string.Empty);
            int copyLength = Math.Min(bytes.Length, fieldSize - 1);
            Array.Copy(bytes, 0, destination, offset, copyLength);
        }

        private static string ReadNullTerminatedString(byte[] source, int offset, int fieldSize)
        {
            int length = 0;
            while (length < fieldSize && source[offset + length] != 0)
            {
                length++;
            }
            return Encoding.UTF8.GetString(source, offset, length);
        }

    }
}
