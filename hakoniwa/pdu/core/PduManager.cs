using System;
using System.Collections.Generic;
using hakoniwa.environment.interfaces;
using hakoniwa.pdu.interfaces;

namespace hakoniwa.pdu.core
{
    public class PduManager
    {
        private IEnvironmentService service;
        private bool enabledService;
        private PduBuffer buffers;
        private PduEncoder encoder;
        private PduDecoder decoder;
        private PduDataDefinitionLoader pdu_definition_loader;
        private PduChannelConfig pdu_channel_config;
        public PduManager(IEnvironmentService srv, string config_path)
        {
            service = srv;
            pdu_definition_loader = new PduDataDefinitionLoader(service.GetFileLoader(), config_path);
            PduChannelLoader pdu_channel_loader= new PduChannelLoader(service.GetFileLoader());
            string fullPath = System.IO.Path.Combine(config_path, "custom");
            pdu_channel_config = pdu_channel_loader.Load(fullPath, ".json");
        }
        public bool StartService()
        {
            if (enabledService)
            {
                return false;
            }
            buffers = new PduBuffer();
            decoder = new PduDecoder(pdu_definition_loader);
            encoder = new PduEncoder(pdu_definition_loader);
            //TODO start service
            enabledService = true;
            return false;
        }
        public bool StopService()
        {
            if (enabledService)
            {
                //TODO stop service
            }
            buffers = null;
            enabledService = false;
            return true;
        }

        public IPdu CreatePdu(string robotName, string pduName)
        {
            if (!enabledService)
            {
                return null;
            }

            string packageName;
            string typeName;
            GetPackageTypeName(robotName, pduName, out packageName, out typeName);

            // 定義をロードし、存在を確認
            var definition = pdu_definition_loader.LoadDefinition(packageName + "/" + typeName);
            if (definition == null)
            {
                throw new ArgumentException($"Definition not found for {packageName}/{typeName}");
            }

            // 定義サイズに基づきバッファを初期化
            var raw_data = new byte[HakoPduMetaDataType.PduMetaDataSize + definition.TotalSize];

            // デコーダーで初期化されたPDUを返す
            return decoder.Decode(pduName, packageName, typeName, raw_data);
        }
        public IPdu CreatePduByType(string pduName, string packageName, string typeName)
        {
            // 定義をロードし、存在を確認
            var definition = pdu_definition_loader.LoadDefinition(packageName + "/" + typeName);
            if (definition == null)
            {
                throw new ArgumentException($"Definition not found for {packageName}/{typeName}");
            }

            // 定義サイズに基づきバッファを初期化
            var raw_data = new byte[HakoPduMetaDataType.PduMetaDataSize + definition.TotalSize];

            // デコーダーで初期化されたPDUを返す
            return decoder.Decode(pduName, packageName, typeName, raw_data);
        }
        public void WritePdu(string robotName, IPdu pdu)
        {
            if (!enabledService)
            {
                throw new ArgumentException($"PduManger Service is not enabled");
            }

            byte[] encodedData = encoder.Encode(pdu as Pdu);
            //Console.WriteLine($"encodedData Len: {encodedData.Length}");
            string key = robotName + "_" + pdu.Name;
            buffers.SetBuffer(key, encodedData);
        }
        public void FlushPdu(IPdu pdu)
        {
            //TODO
        }

        public IPdu ReadPdu(string robotName, string pduName)
        {
            if (!enabledService)
            {
                throw new ArgumentException($"PduManger Service is not enabled");
            }

            string key = robotName + "_" + pduName;
            byte[] raw_data = buffers.GetBuffer(key);
            if (raw_data == null)
            {
                throw new ArgumentException($"PDU type not found for {robotName}/{pduName}");
            }

            string packageName;
            string typeName;
            GetPackageTypeName(robotName, pduName, out packageName, out typeName);

            return decoder.Decode(pduName, packageName, typeName, raw_data);
        }
        // robotNameとpduNameからpackageNameとtypeNameを抽出するメソッド
        private void GetPackageTypeName(string robotName, string pduName, out string packageName, out string typeName)
        {
            // PDUタイプをPDUチャネル設定から取得
            var value = pdu_channel_config.GetPduType(robotName, pduName);
            if (value == null)
            {
                throw new ArgumentException($"PDU type not found for {robotName}/{pduName}");
            }

            // "package/type" の形式であることを確認し、分割
            var parts = value.Split("/");
            if (parts.Length != 2)
            {
                throw new FormatException($"Invalid format for PDU type: {value}");
            }

            // packageNameとtypeNameをアウトパラメータとして設定
            packageName = parts[0];
            typeName = parts[1];
        }
    }
}
