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
    }
}
