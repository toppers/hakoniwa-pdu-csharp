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
            throw new KeyNotFoundException($"Field '{fieldName}' not found in PDU definition.");
        }
    }

    public class PduArrayFieldDefinition
    {
        public bool is_fixed;
        public int ArrayLen { get; set; }
        public int HeapOffset { get; set; }
    }

    public class PduFieldDefinition
    {
        public bool IsArray { get; set; }
        public bool IsVarray { get; set; }
        public bool IsPrimitive { get; set; }
        public string DataKind { get; set; }
        public string MemberName { get; set; }
        public string DataTypeName { get; set; }
        public int ByteOffset { get; set; }
        public int ByteSize { get; set; }
        public PduArrayFieldDefinition ArrayInfo { get; set; }
    }
}
