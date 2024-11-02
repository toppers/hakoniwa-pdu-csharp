using Newtonsoft.Json;
using System.Collections.Generic;
using hakoniwa.environment.interfaces;

namespace hakoniwa.pdu.core
{
    public class PduChannelConfig
    {
        [JsonConstructor]
        public PduChannelConfig() { }
        public List<RobotConfig> robots { get; set; }
        public string GetPduType(string robotName, string pduName)
        {
            foreach (var robot in robots)
            {
                if (robot.name == robotName)
                {
                    // shm_pdu_readersとshm_pdu_writersを検索
                    foreach (var reader in robot.shm_pdu_readers)
                    {
                        if (reader.org_name == pduName)
                        {
                            return reader.type;
                        }
                    }
                    foreach (var writer in robot.shm_pdu_writers)
                    {
                        if (writer.org_name == pduName)
                        {
                            return writer.type;
                        }
                    }
                }
            }
            // 一致する設定がない場合はnullを返す
            return null;
        }
        public string GetPduName(string robotName, int channelId)
        {
            foreach (var robot in robots)
            {
                if (robot.name == robotName)
                {
                    // shm_pdu_readersとshm_pdu_writersを検索
                    foreach (var reader in robot.shm_pdu_readers)
                    {
                        if (reader.channel_id == channelId)
                        {
                            return reader.org_name;
                        }
                    }
                    foreach (var writer in robot.shm_pdu_writers)
                    {
                        if (writer.channel_id == channelId)
                        {
                            return writer.org_name;
                        }
                    }
                }
            }
            // 一致する設定がない場合はnullを返す
            return null;
        }
        public int GetChannelId(string robotName, string pduName)
        {
            foreach (var robot in robots)
            {
                if (robot.name == robotName)
                {
                    // shm_pdu_readersとshm_pdu_writersを検索
                    foreach (var reader in robot.shm_pdu_readers)
                    {
                        if (reader.org_name == pduName)
                        {
                            return reader.channel_id;
                        }
                    }
                    foreach (var writer in robot.shm_pdu_writers)
                    {
                        if (writer.org_name == pduName)
                        {
                            return writer.channel_id;
                        }
                    }
                }
            }
            // 一致する設定がない場合はnullを返す
            return -1;
        }
    }


    public class RobotConfig
    {
        [JsonConstructor]
        public RobotConfig() { }
        public string name { get; set; }
        public List<PduChannel> rpc_pdu_readers { get; set; }
        public List<PduChannel> rpc_pdu_writers { get; set; }
        public List<PduChannel> shm_pdu_readers { get; set; }
        public List<PduChannel> shm_pdu_writers { get; set; }
    }

    public class PduChannel
    {
        [JsonConstructor]
        public PduChannel() { }
        public string type { get; set; }
        public string org_name { get; set; }
        public string name { get; set; }
        public string class_name { get; set; }
        public string conv_class_name { get; set; }
        public int channel_id { get; set; }
        public int pdu_size { get; set; }
        public int write_cycle { get; set; }
        public string method_type { get; set; }
    }
    public class PduChannelLoader
    {
        private IFileLoader fileLoader;

        public PduChannelLoader(IFileLoader loader)
        {
            fileLoader = loader;
        }

        public PduChannelConfig Load(string filePath, string extension)
        {
            string jsonContent = fileLoader.LoadText(filePath, extension);
            PduChannelConfig config = JsonConvert.DeserializeObject<PduChannelConfig>(jsonContent);
            return config;
        }
    }
}
