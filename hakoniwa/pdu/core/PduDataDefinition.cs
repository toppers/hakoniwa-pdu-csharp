using System.Collections.Generic;

namespace hakoniwa.pdu.core
{
    public class PduDataDefinition
    {
        private Dictionary<string, PduFieldDefinition> fieldDefinitions;

        // コンストラクタで初期化、またはデータ読み込みメソッド
        public PduDataDefinition(Dictionary<string, PduFieldDefinition> definitions)
        {
            this.fieldDefinitions = definitions;
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

    public class PduFieldDefinition
    {
        public string ArrayType { get; set; }
        public string DataType { get; set; }
        public string MemberName { get; set; }
        public string DataTypeName { get; set; }
        public int ByteOffset { get; set; }
        public int ByteSize { get; set; }
    }
}
