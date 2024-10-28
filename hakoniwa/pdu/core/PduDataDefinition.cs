using System;
using System.Collections.Generic;

namespace hakoniwa.pdu.core
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
        public uint total_size;
        public HakoPduMetaDataType(byte[] raw_data)
        {
            magicno = BitConverter.ToUInt32(raw_data, 0);
            version = BitConverter.ToUInt32(raw_data, 4);
            base_off = BitConverter.ToUInt32(raw_data, 8);
            heap_off = BitConverter.ToUInt32(raw_data, 12);
            total_size = BitConverter.ToUInt32(raw_data, 16);
        }
    }
}
