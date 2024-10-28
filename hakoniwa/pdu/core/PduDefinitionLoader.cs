using System;
using System.Collections.Generic;
using System.IO;
using hakoniwa.environment.interfaces;

namespace hakoniwa.pdu.core
{
    public class PduDataDefinitionLoader
    {
        private Dictionary<string, PduDataDefinition> definitions = new Dictionary<string, PduDataDefinition>();
        private IFileLoader file_loader;
        private static readonly HashSet<string> PrimitiveTypes = new HashSet<string>
        { "int8", "int16", "int32", "int64", "uint8", "uint16", "uint32", "uint64", "float32", "float64", "bool", "string" };

        public PduDataDefinitionLoader(IFileLoader ldr)
        {
            file_loader = ldr ?? throw new ArgumentNullException(nameof(ldr));
        }

        public PduDataDefinition LoadDefinition(string package_type_name)
        {
            if (definitions.ContainsKey(package_type_name))
            {
                return definitions[package_type_name];
            }

            string textContent = file_loader.LoadText(package_type_name, ".offset");
            if (string.IsNullOrWhiteSpace(textContent))
            {
                throw new FileNotFoundException($"Could not load definition for {package_type_name}");
            }
            var definition = Parse(package_type_name, textContent);
            definitions[package_type_name] = definition;
            return definition;
        }

        private PduDataDefinition Parse(string package_type_name, string textContent)
        {
            string package_name = package_type_name.Split("/")[0];
            string[] lines = textContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            Dictionary<string, PduFieldDefinition> fieldDefinitions = new Dictionary<string, PduFieldDefinition>();
            int total_size = 0;

            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                string[] attr = line.Split(':');
                if (attr.Length < 6)
                    throw new FormatException($"Invalid format in line: {line}");

                var elm = CreateFieldDefinition(attr, package_name);
                fieldDefinitions[elm.MemberName] = elm;

                // 全体の PDU サイズの計算
                total_size = Math.Max(total_size, GetSize(elm, attr));
            }

            var definition = new PduDataDefinition(fieldDefinitions)
            {
                TotalSize = total_size
            };
            return definition;
        }

        private int GetSize(PduFieldDefinition elm, string[] attr)
        {
            if (elm.IsPrimitive)
            {
                return elm.ByteOffset + elm.ByteSize;
            }
            else if (elm.IsArray)
            {
                return elm.ByteOffset + (elm.ByteSize * elm.ArrayInfo.ArrayLen);
            }
            else if (elm.IsVarray)
            {
                return elm.ByteOffset + int.Parse(attr[6]);
            }
            throw new InvalidOperationException($"Unsupported type for field {elm.MemberName}");
        }

        private PduFieldDefinition CreateFieldDefinition(string[] attr, string package_name)
        {
            PduFieldDefinition elm = new PduFieldDefinition
            {
                IsArray = attr[0].Equals("array"),
                IsVarray = attr[0].Equals("varray"),
                IsPrimitive = attr[1].Equals("primitive"),
                MemberName = attr[2],
                DataTypeName = attr[3].Contains("/") || IsPrimitive(attr[3]) ? attr[3] : package_name + "/" + attr[3],
                ByteOffset = int.Parse(attr[4]),
                ByteSize = int.Parse(attr[5])
            };

            if (elm.IsArray || elm.IsVarray)
            {
                elm.ArrayInfo = new PduArrayFieldDefinition
                {
                    ArrayLen = elm.IsArray ? int.Parse(attr[6]) : -1
                };
                elm.ByteSize = elm.IsArray ? elm.ByteSize / elm.ArrayInfo.ArrayLen : elm.ByteSize;
            }
            return elm;
        }

        private static bool IsPrimitive(string type_name)
        {
            return PrimitiveTypes.Contains(type_name);
        }
    }
}
