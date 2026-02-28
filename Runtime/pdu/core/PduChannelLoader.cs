#if !UNITY_WEBGL
using Newtonsoft.Json;
#else
using UnityEngine;
#endif
using System;
using System.Collections.Generic;
using System.IO;
using hakoniwa.environment.interfaces;

namespace hakoniwa.pdu.core
{
#pragma warning disable CS0649
    [System.Serializable]
    internal class CompactPduType
    {
        public int channel_id;
        public int pdu_size;
        public string name;
        public string type;
    }

    [System.Serializable]
    internal class CompactPduTypePath
    {
        public string id;
        public string path;
    }

    [System.Serializable]
    internal class CompactRobotPduConfig
    {
        public string name;
        public string pdutypes_id;
    }

    [System.Serializable]
    internal class CompactPduChannelConfig
    {
        public List<CompactPduTypePath> paths = new List<CompactPduTypePath>();
        public List<CompactRobotPduConfig> robots = new List<CompactRobotPduConfig>();
    }

    internal class PduDefinitionEntry
    {
        public string type;
        public string org_name;
        public string name;
        public string class_name;
        public string conv_class_name;
        public int channel_id;
        public int pdu_size;
        public int write_cycle;
        public string method_type;
    }

    public class PduChannelConfig
    {
        private readonly Dictionary<string, Dictionary<string, PduDefinitionEntry>> pduDefinitionsByName =
            new Dictionary<string, Dictionary<string, PduDefinitionEntry>>();
        private readonly Dictionary<string, Dictionary<int, PduDefinitionEntry>> pduDefinitionsByChannel =
            new Dictionary<string, Dictionary<int, PduDefinitionEntry>>();

        internal void AddDefinition(string robotName, PduDefinitionEntry definition)
        {
            if (!pduDefinitionsByName.TryGetValue(robotName, out var byName))
            {
                byName = new Dictionary<string, PduDefinitionEntry>();
                pduDefinitionsByName[robotName] = byName;
            }
            if (!pduDefinitionsByChannel.TryGetValue(robotName, out var byChannel))
            {
                byChannel = new Dictionary<int, PduDefinitionEntry>();
                pduDefinitionsByChannel[robotName] = byChannel;
            }

            byName[definition.org_name] = definition;
            byChannel[definition.channel_id] = definition;
        }

        public string GetPduType(string robotName, string pduName)
        {
            if (pduDefinitionsByName.TryGetValue(robotName, out var byName) &&
                byName.TryGetValue(pduName, out var definition))
            {
                return definition.type;
            }
            return null;
        }

        public string GetPduName(string robotName, int channelId)
        {
            if (pduDefinitionsByChannel.TryGetValue(robotName, out var byChannel) &&
                byChannel.TryGetValue(channelId, out var definition))
            {
                return definition.org_name;
            }
            return null;
        }

        public int GetChannelId(string robotName, string pduName)
        {
            if (pduDefinitionsByName.TryGetValue(robotName, out var byName) &&
                byName.TryGetValue(pduName, out var definition))
            {
                return definition.channel_id;
            }
            return -1;
        }
        public int GetPduSize(string robotName, string pduName)
        {
            if (pduDefinitionsByName.TryGetValue(robotName, out var byName) &&
                byName.TryGetValue(pduName, out var definition))
            {
                return definition.pdu_size;
            }
            return -1;
        }
    }

    [System.Serializable]
    internal class LegacyPduChannelConfig
    {
#if UNITY_WEBGL
        public List<RobotConfig> robots = new List<RobotConfig>();
#else
        [Newtonsoft.Json.JsonProperty]
        public List<RobotConfig> robots { get; set; } = new List<RobotConfig>();
#endif
    }
#pragma warning restore CS0649

    [System.Serializable]
    public class RobotConfig
    {
        public string name;
        public List<PduChannel> rpc_pdu_readers = new List<PduChannel>();
        public List<PduChannel> rpc_pdu_writers = new List<PduChannel>();
        public List<PduChannel> shm_pdu_readers = new List<PduChannel>();
        public List<PduChannel> shm_pdu_writers = new List<PduChannel>();
    }

    [System.Serializable]
    public class PduChannel
    {
        public string type;
        public string org_name;
        public string name;
        public string class_name;
        public string conv_class_name;
        public int channel_id;
        public int pdu_size;
        public int write_cycle;
        public string method_type;
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
            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                return null;
            }

            string baseDir = Path.GetDirectoryName(filePath) ?? ".";
            if (IsCompactFormat(jsonContent))
            {
                return LoadCompact(jsonContent, baseDir);
            }

            return LoadLegacy(jsonContent);
        }

        private static PduChannelConfig LoadLegacy(string jsonContent)
        {
#if UNITY_WEBGL
            LegacyPduChannelConfig legacy = JsonUtility.FromJson<LegacyPduChannelConfig>(jsonContent);
#else
            LegacyPduChannelConfig legacy = Newtonsoft.Json.JsonConvert.DeserializeObject<LegacyPduChannelConfig>(jsonContent);
#endif
            if (legacy == null)
            {
                return null;
            }

            var config = new PduChannelConfig();
            foreach (var robot in legacy.robots)
            {
                AddLegacyDefinitions(config, robot.name, robot.shm_pdu_readers);
                AddLegacyDefinitions(config, robot.name, robot.shm_pdu_writers);
            }
            return config;
        }

        private static bool IsCompactFormat(string jsonContent)
        {
#if UNITY_WEBGL
            return jsonContent.Contains("\"paths\"");
#else
            var root = Newtonsoft.Json.Linq.JObject.Parse(jsonContent);
            return root["paths"] != null;
#endif
        }

        private PduChannelConfig LoadCompact(string jsonContent, string baseDir)
        {
#if UNITY_WEBGL
            CompactPduChannelConfig compact = JsonUtility.FromJson<CompactPduChannelConfig>(jsonContent);
#else
            CompactPduChannelConfig compact = Newtonsoft.Json.JsonConvert.DeserializeObject<CompactPduChannelConfig>(jsonContent);
#endif
            if (compact == null)
            {
                return null;
            }

            var typeSetMap = new Dictionary<string, List<PduDefinitionEntry>>();
            foreach (var pathEntry in compact.paths)
            {
                string resolvedPath = pathEntry.path;
                if (!Path.IsPathRooted(resolvedPath))
                {
                    resolvedPath = Path.Combine(baseDir, resolvedPath);
                }
                string pdutypesJson = fileLoader.LoadText(RemoveJsonExtension(resolvedPath), ".json");
                typeSetMap[pathEntry.id] = LoadCompactPduTypes(pdutypesJson);
            }

            var config = new PduChannelConfig();
            foreach (var robotEntry in compact.robots)
            {
                if (!typeSetMap.TryGetValue(robotEntry.pdutypes_id, out var definitions))
                {
                    throw new ArgumentException($"PDU types ID not found: {robotEntry.pdutypes_id}");
                }
                foreach (var definition in definitions)
                {
                    config.AddDefinition(robotEntry.name, CloneDefinition(definition));
                }
            }
            return config;
        }

        private static void AddLegacyDefinitions(PduChannelConfig config, string robotName, List<PduChannel> channels)
        {
            if (channels == null)
            {
                return;
            }
            foreach (var channel in channels)
            {
                config.AddDefinition(robotName, new PduDefinitionEntry
                {
                    type = channel.type,
                    org_name = channel.org_name,
                    name = channel.name,
                    class_name = channel.class_name,
                    conv_class_name = channel.conv_class_name,
                    channel_id = channel.channel_id,
                    pdu_size = channel.pdu_size,
                    write_cycle = channel.write_cycle,
                    method_type = channel.method_type
                });
            }
        }

        private static List<PduDefinitionEntry> LoadCompactPduTypes(string pdutypesJson)
        {
#if UNITY_WEBGL
            CompactPduTypeArrayWrapper wrapper = JsonUtility.FromJson<CompactPduTypeArrayWrapper>("{\"items\":" + pdutypesJson + "}");
            List<CompactPduType> pdutypes = wrapper?.items ?? new List<CompactPduType>();
#else
            List<CompactPduType> pdutypes = Newtonsoft.Json.JsonConvert.DeserializeObject<List<CompactPduType>>(pdutypesJson);
#endif
            var definitions = new List<PduDefinitionEntry>();
            foreach (var pduType in pdutypes)
            {
                definitions.Add(new PduDefinitionEntry
                {
                    type = pduType.type,
                    org_name = pduType.name,
                    name = pduType.name,
                    channel_id = pduType.channel_id,
                    pdu_size = pduType.pdu_size,
                    method_type = "SHM"
                });
            }
            return definitions;
        }

        private static PduDefinitionEntry CloneDefinition(PduDefinitionEntry definition)
        {
            return new PduDefinitionEntry
            {
                type = definition.type,
                org_name = definition.org_name,
                name = definition.name,
                class_name = definition.class_name,
                conv_class_name = definition.conv_class_name,
                channel_id = definition.channel_id,
                pdu_size = definition.pdu_size,
                write_cycle = definition.write_cycle,
                method_type = definition.method_type
            };
        }

        private static string RemoveJsonExtension(string path)
        {
            if (path.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                return path.Substring(0, path.Length - 5);
            }
            return path;
        }

#if UNITY_WEBGL
        [System.Serializable]
        private class CompactPduTypeArrayWrapper
        {
            public List<CompactPduType> items = new List<CompactPduType>();
        }
#endif
    }
}
