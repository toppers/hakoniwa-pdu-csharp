using System;
using System.Collections.Generic;

namespace hakoniwa.pdu.interfaces
{
    public class PduDataDefinition
    {
        public int TotalSize { get; set; }
        private Dictionary<string, PduFieldDefinition> fieldDefinitions;

        public PduDataDefinition(Dictionary<string, PduFieldDefinition> pdu_field_definitions)
        {
            this.fieldDefinitions = pdu_field_definitions;
        }

        public PduFieldDefinition GetFieldDefinition(string fieldName)
        {
            if (fieldDefinitions.ContainsKey(fieldName))
            {
                return fieldDefinitions[fieldName];
            }
            return null;
        }
        public Dictionary<string, PduFieldDefinition> Get()
        {
            return fieldDefinitions;
        }

    }
    public class PduArrayFieldDefinition
    {
        public int ArrayLen { get; set; }
    }

    public class PduFieldDefinition
    {
        public enum FieldType
        {
            Single,       // 配列ではない単一のメンバ
            FixedArray,   // 固定長の配列
            VariableArray // 可変長の配列
        }
        public FieldType Type { get; set; }
        public bool IsPrimitive { get; set; }
        public string MemberName { get; set; }
        //format: "<package_name>/<type_name>"
        public string DataTypeName { get; set; }
        public int ByteMemberOffset { get; set; }
        public int ByteMemberDataTypeSize { get; set; }
        public PduArrayFieldDefinition ArrayInfo { get; set; }

        public string GetPackageName()
        {
            return DataTypeName.Split("/")[0];
        }
        public string GetTypeName()
        {
            return DataTypeName.Split("/")[1];
        }
    }
    public class HakoPduMetaDataType
    {
        public static readonly int PduMetaDataSize = 24;
        public static readonly uint PduMetaDataMagicNo = 0x12345678;
        public static readonly uint PduMetaDataVersion = 1;
        public uint magicno { get; set; }
        public uint version { get; set; }
        public uint base_off { get; set; }
        public uint heap_off { get; set; }
        public uint total_size { get; set; }
        public HakoPduMetaDataType(byte[] raw_data)
        {
            magicno = BitConverter.ToUInt32(raw_data, 0);
            version = BitConverter.ToUInt32(raw_data, 4);
            base_off = BitConverter.ToUInt32(raw_data, 8);
            heap_off = BitConverter.ToUInt32(raw_data, 12);
            total_size = BitConverter.ToUInt32(raw_data, 16);
        }
        public HakoPduMetaDataType(uint base_size)
        {
            magicno = PduMetaDataMagicNo;
            version = PduMetaDataVersion;
            base_off = (uint)PduMetaDataSize;
            heap_off = (uint)PduMetaDataSize + base_size;
            total_size = 0;
        }
        public void SetMetaDataToBuffer(byte[] buffer)
        {
            Buffer.BlockCopy(BitConverter.GetBytes(magicno), 0, buffer, 0, sizeof(uint));
            Buffer.BlockCopy(BitConverter.GetBytes(version), 0, buffer, 4, sizeof(uint));
            Buffer.BlockCopy(BitConverter.GetBytes(base_off), 0, buffer, 8, sizeof(uint));
            Buffer.BlockCopy(BitConverter.GetBytes(heap_off), 0, buffer, 12, sizeof(uint));
            Buffer.BlockCopy(BitConverter.GetBytes(total_size), 0, buffer, 16, sizeof(uint));
        }
    }
}
