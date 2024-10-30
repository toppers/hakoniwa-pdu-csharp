using System;
using System.Text;
using hakoniwa.environment.interfaces;

namespace hakoniwa.environment.impl
{
    public class DataPacket: IDataPacket
    {
        public string RobotName { get; set; }
        public uint ChannelId { get; set; }
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
        ///     3. チャネルID (uint32)
        /// - ボディ部: 残りのデータはプロトコルでの解釈は行わず、バイナリデータとして保持
        /// </summary>
        /// <param name="data">解析するデータパケット</param>
        /// <returns>解析結果を保持する ParsedData オブジェクト</returns>
        /// <exception cref="ArgumentException">データの長さが不足している場合</exception>
        public static DataPacket Decode(byte[] data)
        {
            if (data.Length < 12)
                throw new ArgumentException("データが短すぎます。");

            int currentIndex = 0;
            int totalLength = BitConverter.ToInt32(data, currentIndex);
            currentIndex += 4;

            int headerLength = BitConverter.ToInt32(data, currentIndex);
            currentIndex += 4;

            if (data.Length < 8 + headerLength)
                throw new ArgumentException("指定されたヘッダー長が不正です。");

            int robotNameLength = BitConverter.ToInt32(data, currentIndex);
            currentIndex += 4;

            string robotName = Encoding.UTF8.GetString(data, currentIndex, robotNameLength);
            currentIndex += robotNameLength;

            uint channelId = BitConverter.ToUInt32(data, currentIndex);
            currentIndex += 4;

            int bodyStartIndex = currentIndex;
            int bodyLength = totalLength - bodyStartIndex;
            byte[] bodyData = new byte[bodyLength];
            Array.Copy(data, bodyStartIndex, bodyData, 0, bodyLength);

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
        public static byte[] Encode(DataPacket packet)
        {
            byte[] robotNameBytes = Encoding.UTF8.GetBytes(packet.RobotName);
            int robotNameLength = robotNameBytes.Length;

            int headerLength = 4 + robotNameLength + 4;
            int totalLength = 8 + headerLength + (packet.BodyData?.Length ?? 0);

            byte[] data = new byte[totalLength];
            int currentIndex = 0;

            BitConverter.GetBytes(totalLength).CopyTo(data, currentIndex);
            currentIndex += 4;

            BitConverter.GetBytes(headerLength).CopyTo(data, currentIndex);
            currentIndex += 4;

            BitConverter.GetBytes(robotNameLength).CopyTo(data, currentIndex);
            currentIndex += 4;

            robotNameBytes.CopyTo(data, currentIndex);
            currentIndex += robotNameLength;

            BitConverter.GetBytes(packet.ChannelId).CopyTo(data, currentIndex);
            currentIndex += 4;

            packet.BodyData?.CopyTo(data, currentIndex);

            return data;
        }

    }
}

