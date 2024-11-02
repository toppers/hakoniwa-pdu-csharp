using Newtonsoft.Json;
using System.Collections.Generic;
using hakoniwa.environment.interfaces;

namespace hakoniwa.pdu.core
{
    public class PduChannelConfig
    {
        [JsonConstructor]
        public PduChannelConfig(List<RobotConfig> robots)
        {
            this.robots = robots ?? new List<RobotConfig>();
        }

        public List<RobotConfig> robots { get; set; } = new List<RobotConfig>();

        public string GetPduType(string robotName, string pduName)
        {
            foreach (var robot in robots)
            {
                if (robot.name == robotName)
                {
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
            return null;
        }

        public string GetPduName(string robotName, int channelId)
        {
            foreach (var robot in robots)
            {
                if (robot.name == robotName)
                {
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
            return null;
        }

        public int GetChannelId(string robotName, string pduName)
        {
            foreach (var robot in robots)
            {
                if (robot.name == robotName)
                {
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
            return -1;
        }
    }

    public class RobotConfig
    {
        [JsonConstructor]
        public RobotConfig(string name, List<PduChannel> rpc_pdu_readers, List<PduChannel> rpc_pdu_writers, List<PduChannel> shm_pdu_readers, List<PduChannel> shm_pdu_writers)
        {
            this.name = name;
            this.rpc_pdu_readers = rpc_pdu_readers ?? new List<PduChannel>();
            this.rpc_pdu_writers = rpc_pdu_writers ?? new List<PduChannel>();
            this.shm_pdu_readers = shm_pdu_readers ?? new List<PduChannel>();
            this.shm_pdu_writers = shm_pdu_writers ?? new List<PduChannel>();
        }

        public string name { get; set; }
        public List<PduChannel> rpc_pdu_readers { get; set; } = new List<PduChannel>();
        public List<PduChannel> rpc_pdu_writers { get; set; } = new List<PduChannel>();
        public List<PduChannel> shm_pdu_readers { get; set; } = new List<PduChannel>();
        public List<PduChannel> shm_pdu_writers { get; set; } = new List<PduChannel>();
    }

    public class PduChannel
    {
        [JsonConstructor]
        public PduChannel(string type, string org_name, string name, string class_name, string conv_class_name, int channel_id, int pdu_size, int write_cycle, string method_type)
        {
            this.type = type;
            this.org_name = org_name;
            this.name = name;
            this.class_name = class_name;
            this.conv_class_name = conv_class_name;
            this.channel_id = channel_id;
            this.pdu_size = pdu_size;
            this.write_cycle = write_cycle;
            this.method_type = method_type;
        }

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
