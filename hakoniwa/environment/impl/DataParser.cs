using System;
using System.Text;

namespace hakoniwa.environment.impl
{
    public class ParsedData
    {
        public string RobotName { get; set; }
        public uint ChannelId { get; set; }
        public byte[] BodyData { get; set; }
    }

    public class DataParser
    {
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
        public ParsedData Parse(byte[] data)
        {
            if (data.Length < 12) // データ長、ヘッダ長、ロボット名長を含む最低限のサイズ
                throw new ArgumentException("データが短すぎます。");

            int currentIndex = 0;

            // 先頭4バイト: 後続するデータ全体の長さを取得
            int totalLength = BitConverter.ToInt32(data, currentIndex);
            currentIndex += 4;

            // 次の4バイト: ヘッダデータの長さを取得
            int headerLength = BitConverter.ToInt32(data, currentIndex);
            currentIndex += 4;

            // データ長チェック: ヘッダ長が指定された分あるか確認
            if (data.Length < 8 + headerLength)
                throw new ArgumentException("指定されたヘッダー長が不正です。");

            // ヘッダデータ解析
            // 1. ロボット名長（uint32）
            int robotNameLength = BitConverter.ToInt32(data, currentIndex);
            currentIndex += 4;

            // 2. ロボット名（可変長文字列、UTF-8デコード）
            string robotName = Encoding.UTF8.GetString(data, currentIndex, robotNameLength);
            currentIndex += robotNameLength;

            // 3. チャネルID（uint32）
            uint channelId = BitConverter.ToUInt32(data, currentIndex);
            currentIndex += 4;

            // ボディ部を取得: 残りのデータ部分をバイナリデータとしてコピー
            int bodyStartIndex = currentIndex;
            int bodyLength = totalLength - bodyStartIndex;
            byte[] bodyData = new byte[bodyLength];
            Array.Copy(data, bodyStartIndex, bodyData, 0, bodyLength);

            // 解析結果を返す
            return new ParsedData
            {
                RobotName = robotName,
                ChannelId = channelId,
                BodyData = bodyData
            };
        }
    }
}

