﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using hakoniwa.environment.interfaces;
using hakoniwa.pdu.interfaces;

namespace hakoniwa.pdu.core
{
    public class PduManager: IPduManager
    {
        private IEnvironmentService service;
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
        public async Task<bool> StartService()
        {
            if (service.GetCommunication().IsServiceEnabled())
            {
                return false;
            }
            buffers = new PduBuffer(pdu_channel_config);
            decoder = new PduDecoder(pdu_definition_loader);
            encoder = new PduEncoder(pdu_definition_loader);

            await service.GetCommunication().StartService(buffers);
            return false;
        }
        public bool StopService()
        {
            if (!service.GetCommunication().IsServiceEnabled())
            {
                return false;
            }

            service.GetCommunication().StopService();
            buffers = null;
            return true;
        }

        public IPdu CreatePdu(string robotName, string pduName)
        {
            if (!service.GetCommunication().IsServiceEnabled())
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
            if (!service.GetCommunication().IsServiceEnabled())
            {
                return null;
            }
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
        public string WritePdu(string robotName, IPdu pdu)
        {
            if (!service.GetCommunication().IsServiceEnabled())
            {
                return null;
            }

            byte[] encodedData = encoder.Encode(pdu as Pdu);
            //Console.WriteLine($"encodedData Len: {encodedData.Length}");
            string key = GetKey(robotName, pdu.Name);
            buffers.SetBuffer(key, encodedData);
            return key;
        }
        public async Task<bool> FlushPdu(string robotName, string pduName)
        {
            if (!service.GetCommunication().IsServiceEnabled())
            {
                return false;
            }
            string key = GetKey(robotName, pduName);
            byte[] pdu_raw_data = buffers.GetBuffer(key);
            if (pdu_raw_data == null)
            {
                return false;
            }
            int channel_id = pdu_channel_config.GetChannelId(robotName, pduName);
            if (channel_id < 0)
            {
                throw new ArgumentException($"PDU channel ID not found for {key}");
            }
            await service.GetCommunication().SendData(robotName, channel_id, pdu_raw_data);
            return true;
        }

        public IPdu ReadPdu(string robotName, string pduName)
        {
            if (!service.GetCommunication().IsServiceEnabled())
            {
                return null;
            }

            string key = GetKey(robotName, pduName);
            byte[] raw_data = buffers.GetBuffer(key);
            if (raw_data == null)
            {
                return null;
            }

            string packageName;
            string typeName;
            GetPackageTypeName(robotName, pduName, out packageName, out typeName);

            return decoder.Decode(pduName, packageName, typeName, raw_data);
        }
        public string GetKey(string robotName, string pduName)
        {
            return robotName + "_" + pduName;
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
