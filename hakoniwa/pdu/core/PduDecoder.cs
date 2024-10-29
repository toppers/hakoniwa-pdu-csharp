﻿using System;
using hakoniwa.pdu.interfaces;

namespace hakoniwa.pdu.core
{
    public class PduDecoder
    {
        private PduDataDefinitionLoader loader;
        public PduDecoder(PduDataDefinitionLoader ldr)
        {
            loader = ldr;
        }
        public Pdu Decode(string name, string packageName, string typeName, byte[] raw_data)
        {
            var definition = loader.LoadDefinition(packageName + "/" + typeName);
            Pdu dst = new Pdu(name, packageName, typeName, definition);
            var meta = new HakoPduMetaDataType(raw_data);
            ConvertFromStruct(dst, meta, HakoPduMetaDataType.PduMetaDataSize, raw_data);
            return dst;
        }
        private void ConvertFromStruct(IPdu dst, HakoPduMetaDataType meta, int base_off, byte[] src_buffer)
        {
            foreach (var (fieldName, elm) in dst.GetPduDefinition().Get())
            {
                if (elm.IsPrimitive)
                {
                    //primitive
                    if (elm.Type == PduFieldDefinition.FieldType.FixedArray)
                    {
                        ConvertFromPrimtiveArray(dst, elm, base_off, elm.ByteMemberOffset, elm.ByteMemberDataTypeSize, src_buffer);
                    }
                    else if (elm.Type == PduFieldDefinition.FieldType.VariableArray)
                    {
                        int array_size = BitConverter.ToInt32(src_buffer, base_off + elm.ByteMemberOffset);
                        int offset_from_heap = BitConverter.ToInt32(src_buffer, base_off + elm.ByteMemberOffset + 4);
                        ConvertFromPrimtiveArray(dst, elm, (int)meta.heap_off, offset_from_heap, array_size, src_buffer);
                    }
                    else
                    {
                        ConvertFromPrimtive(dst, elm, base_off, elm.ByteMemberOffset, src_buffer);
                    }
                }
                else
                {
                    //struct
                    if (elm.Type == PduFieldDefinition.FieldType.FixedArray)
                    {
                        ConvertFromStructArray(dst, meta, elm, base_off, elm.ByteMemberOffset, elm.ByteMemberDataTypeSize, src_buffer);
                    }
                    else if (elm.Type == PduFieldDefinition.FieldType.VariableArray)
                    {
                        int array_size = BitConverter.ToInt32(src_buffer, base_off + elm.ByteMemberOffset);
                        int offset_from_heap = BitConverter.ToInt32(src_buffer, base_off + elm.ByteMemberOffset + 4);
                        ConvertFromStructArray(dst, meta, elm, (int)meta.heap_off, offset_from_heap, array_size, src_buffer);
                    }
                    else
                    {
                        PduDataDefinition def = loader.LoadDefinition(elm.DataTypeName);
                        Pdu child_dst = new Pdu(elm.MemberName, elm.GetPackageName(), elm.GetTypeName(), def);
                        ConvertFromStruct(child_dst, meta, base_off + elm.ByteMemberOffset, src_buffer);
                        dst.SetData<IPdu>(elm.MemberName, child_dst);
                    }
                }
            }
        }
        private void ConvertFromStructArray(IPdu dst, HakoPduMetaDataType meta, PduFieldDefinition elm, int base_off, int elm_off, int array_size, byte[] src_buffer)
        {
            PduDataDefinition def = loader.LoadDefinition(elm.DataTypeName);
            Pdu[] child_pdus = new Pdu[array_size];
            for (int i = 0; i < array_size; i++)
            {
                child_pdus[i] = new Pdu(elm.MemberName, elm.GetPackageName(), elm.GetTypeName(), def);
                ConvertFromStruct(child_pdus[i], meta, (base_off + elm_off) + (i * elm.ByteMemberDataTypeSize), src_buffer);
            }
            dst.SetData(elm.MemberName, child_pdus);
        }
        private void ConvertFromPrimtive(IPdu dst, PduFieldDefinition elm, int base_off, int elm_off, byte[] src_buffer)
        {
            var off = base_off + elm_off;
            switch (elm.DataTypeName)
            {
                case "int8":
                    dst.SetData(elm.MemberName, (sbyte)src_buffer[off]);
                    break;
                case "int16":
                    dst.SetData(elm.MemberName, BitConverter.ToInt16(src_buffer, off));
                    break;
                case "int32":
                    dst.SetData(elm.MemberName, BitConverter.ToInt32(src_buffer, off));
                    break;
                case "int64":
                    dst.SetData(elm.MemberName, BitConverter.ToInt64(src_buffer, off));
                    break;
                case "uint8":
                    dst.SetData(elm.MemberName, (byte)src_buffer[off]);
                    break;
                case "uint16":
                    dst.SetData(elm.MemberName, BitConverter.ToUInt16(src_buffer, off));
                    break;
                case "uint32":
                    dst.SetData(elm.MemberName, BitConverter.ToUInt32(src_buffer, off));
                    break;
                case "uint64":
                    dst.SetData(elm.MemberName, BitConverter.ToUInt64(src_buffer, off));
                    break;
                case "float32":
                    dst.SetData(elm.MemberName, BitConverter.ToSingle(src_buffer, off));
                    break;
                case "float64":
                    dst.SetData(elm.MemberName, BitConverter.ToDouble(src_buffer, off));
                    break;
                case "bool":
                    dst.SetData(elm.MemberName, BitConverter.ToBoolean(src_buffer, off));
                    break;
                case "string":
                    int nullIndex = Array.IndexOf(src_buffer, (byte)0);
                    var bytes = new byte[nullIndex];
                    Buffer.BlockCopy(src_buffer, off, bytes, 0, bytes.Length);
                    dst.SetData(elm.MemberName,
                        System.Text.Encoding.ASCII.GetString(bytes));
                    break;
                default:
                    throw new InvalidCastException("Error: Can not found ptype: " + elm.MemberName);
            }
        }
        private static void ConvertFromPrimtiveArray(IPdu dst, PduFieldDefinition elm, int base_off, int elm_off, int array_size, byte[] src_buffer)
        {
            int roff = base_off + elm_off;
            for (int i = 0; i < array_size; i++)
            {
                //SimpleLogger.Get().Log(Level.INFO, "field=" + elm.field_name);
                //SimpleLogger.Get().Log(Level.INFO, "type=" + elm.type_name);
                var off = (roff + i * elm.ByteMemberDataTypeSize);
                switch (elm.DataTypeName)
                {
                    case "int8":
                        dst.SetData(elm.MemberName, i, (sbyte)src_buffer[off]);
                        break;
                    case "int16":
                        dst.SetData(elm.MemberName, i, BitConverter.ToInt16(src_buffer, off));
                        break;
                    case "int32":
                        dst.SetData(elm.MemberName, i, BitConverter.ToInt32(src_buffer, off));
                        break;
                    case "int64":
                        dst.SetData(elm.MemberName, i, BitConverter.ToInt64(src_buffer, off));
                        break;
                    case "uint8":
                        dst.SetData(elm.MemberName, i, (byte)src_buffer[off]);
                        break;
                    case "uint16":
                        dst.SetData(elm.MemberName, i, BitConverter.ToUInt16(src_buffer, off));
                        break;
                    case "uint32":
                        dst.SetData(elm.MemberName, i, BitConverter.ToUInt32(src_buffer, off));
                        break;
                    case "uint64":
                        dst.SetData(elm.MemberName, i, BitConverter.ToUInt64(src_buffer, off));
                        break;
                    case "float32":
                        dst.SetData(elm.MemberName, i, BitConverter.ToSingle(src_buffer, off));
                        break;
                    case "float64":
                        dst.SetData(elm.MemberName, i, BitConverter.ToDouble(src_buffer, off));
                        break;
                    case "bool":
                        dst.SetData(elm.MemberName, i, BitConverter.ToBoolean(src_buffer, off));
                        break;
                    case "string":
                        var bytes = new byte[elm.ByteMemberDataTypeSize];
                        Buffer.BlockCopy(src_buffer, off, bytes, 0, bytes.Length);
                        dst.SetData(elm.MemberName, i,
                            System.Text.Encoding.ASCII.GetString(bytes));
                        break;
                    default:
                        throw new InvalidCastException("Error: Can not found ptype: " + elm.DataTypeName);
                }
            }
        }
    }
}