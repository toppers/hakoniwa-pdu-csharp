using System;
using System.Collections.Generic;
using System.Text;
using hakoniwa.pdu.interfaces;

namespace hakoniwa.pdu.core
{
    public class DynamicAllocator
    {
        private List<byte> data;
        private bool is_heap;

        public DynamicAllocator(bool is_heap)
        {
            this.is_heap = is_heap;
            data = new List<byte>();
        }

        public void Add(byte[] bytes)
        {
            data.AddRange(bytes);
        }

        public void Add(byte[] bytes, int expectedOffset, int count)
        {
            if (is_heap == false)
            {
                int currentSize = data.Count;
                if (currentSize < expectedOffset)
                {
                    data.AddRange(new byte[expectedOffset - currentSize]);
                }
            }
            data.AddRange(new ArraySegment<byte>(bytes, 0, count));
        }

        public byte[] ToArray()
        {
            return data.ToArray();
        }

        public int Size => data.Count;
    }

    public class PduEncoder
    {
        private PduDataDefinitionLoader loader;
        private DynamicAllocator heap_allocator = null;

        public PduEncoder(PduDataDefinitionLoader ldr)
        {
            loader = ldr;
        }

        public byte[] Encode(Pdu src)
        {
            string package_type_name = src.GetPackageTypeName();
            var def = loader.LoadDefinition(package_type_name);
            if (def == null)
            {
                throw new InvalidOperationException("Error: Can not found offset: type=" + package_type_name);
            }
            DynamicAllocator base_allocator = new DynamicAllocator(false);
            heap_allocator = new DynamicAllocator(true);
            HakoPduMetaDataType meta = new HakoPduMetaDataType((uint)def.TotalSize);

            // データを動的アロケータに追加
            ConvertFromStruct(0, base_allocator, src);

            // 全体サイズを計算し、バッファを確保
            int totalSize = def.TotalSize + heap_allocator.Size + HakoPduMetaDataType.PduMetaDataSize;
            byte[] buffer = new byte[totalSize];
            meta.total_size = (uint)totalSize;

            // 基本データをバッファにコピー
            byte[] baseData = base_allocator.ToArray();
            //SimpleLogger.Get().Log(Level.INFO, "base writer: off: " + meta.base_off + "src.len:" + baseData.Length + "dst.len: " + buffer.Length);
            Array.Copy(baseData, 0, buffer, HakoPduMetaDataType.PduMetaDataSize, baseData.Length);

            if (heap_allocator.Size > 0)
            {
                byte[] heapData = heap_allocator.ToArray();
                Array.Copy(heapData, 0, buffer, (int)meta.heap_off, heapData.Length);
            }
            // メタデータをバッファに設定
            meta.SetMetaDataToBuffer(buffer);

            return buffer;
        }

        private void ConvertFromStruct(int parent_off, DynamicAllocator allocator, IPdu src)
        {
            foreach (var (fieldName, elm) in src.GetPduDefinition().Get())
            {
                if (elm.IsPrimitive)
                {
                    //primitive
                    if (elm.Type == PduFieldDefinition.FieldType.FixedArray)
                    {
                        ConvertFromPrimtiveArray(parent_off, elm, allocator, src);
                    }
                    else if (elm.Type == PduFieldDefinition.FieldType.VariableArray)
                    {
                        int offset_from_heap = heap_allocator.Size;
                        int array_size = ConvertFromPrimtiveArray(0, elm, heap_allocator, src);
                        var offset_from_heap_bytes = BitConverter.GetBytes(offset_from_heap);
                        var array_size_bytes = BitConverter.GetBytes(array_size);
                        allocator.Add(array_size_bytes, parent_off + elm.ByteMemberOffset, array_size_bytes.Length);
                        allocator.Add(offset_from_heap_bytes, parent_off + elm.ByteMemberOffset + array_size_bytes.Length, offset_from_heap_bytes.Length);
                    }
                    else
                    {
                        ConvertFromPrimtive(parent_off, elm, allocator, src);
                    }
                }
                else
                {
                    //struct
                    if (elm.Type == PduFieldDefinition.FieldType.FixedArray)
                    {
                        ConvertFromStructArray(parent_off + elm.ByteMemberOffset, elm, allocator, src);
                    }
                    else if (elm.Type == PduFieldDefinition.FieldType.VariableArray)
                    {
                        int offset_from_heap = heap_allocator.Size;
                        int array_size = ConvertFromStructArray(0, elm, heap_allocator, src);
                        var offset_from_heap_bytes = BitConverter.GetBytes(offset_from_heap);
                        var array_size_bytes = BitConverter.GetBytes(array_size);
                        allocator.Add(array_size_bytes, parent_off + elm.ByteMemberOffset, array_size_bytes.Length);
                        allocator.Add(offset_from_heap_bytes, parent_off + elm.ByteMemberOffset + array_size_bytes.Length, offset_from_heap_bytes.Length);
                    }
                    else
                    {
                        ConvertFromStruct(parent_off + elm.ByteMemberOffset, allocator, src.GetData<IPdu>(elm.MemberName));
                    }
                }
            }
        }

        private int ConvertFromStructArray(int parent_off, PduFieldDefinition elm, DynamicAllocator allocator, IPdu src)
        {
            PduDataDefinition def = loader.LoadDefinition(elm.DataTypeName);
            IPdu[] pdus = src.GetDataArray<IPdu>(elm.MemberName);
            int array_size = pdus.Length;
            for (int i = 0; i < array_size; i++)
            {
                IPdu src_data = pdus[i];
                ConvertFromStruct(parent_off + (i * elm.ByteMemberDataTypeSize), allocator, src_data);
            }
            return array_size;
        }

        private int ConvertFromPrimtiveArray(int parent_off, PduFieldDefinition elm, DynamicAllocator allocator, IPdu src)
        {
            int array_size = 0;
            int element_size = elm.ByteMemberDataTypeSize;
            byte[] tmp_bytes = null;

            switch (elm.DataTypeName)
            {
                case "int8":
                    sbyte[] int8Array = src.GetDataArray<SByte>(elm.MemberName);
                    array_size = int8Array.Length;
                    tmp_bytes = new byte[array_size * element_size];
                    Buffer.BlockCopy(int8Array, 0, tmp_bytes, 0, array_size);
                    allocator.Add(tmp_bytes, parent_off + elm.ByteMemberOffset, array_size * element_size);
                    return array_size;
                case "int16":
                    short[] int16Array = src.GetDataArray<Int16>(elm.MemberName);
                    array_size = int16Array.Length;
                    tmp_bytes = new byte[array_size * element_size];
                    Buffer.BlockCopy(int16Array, 0, tmp_bytes, 0, tmp_bytes.Length);
                    allocator.Add(tmp_bytes, parent_off + elm.ByteMemberOffset, tmp_bytes.Length);
                    return array_size;
                case "int32":
                    int[] int32Array = src.GetDataArray<Int32>(elm.MemberName);
                    array_size = int32Array.Length;
                    tmp_bytes = new byte[array_size * element_size];
                    Buffer.BlockCopy(int32Array, 0, tmp_bytes, 0, tmp_bytes.Length);
                    allocator.Add(tmp_bytes, parent_off + elm.ByteMemberOffset, tmp_bytes.Length);
                    return array_size;
                case "int64":
                    long[] int64Array = src.GetDataArray<Int64>(elm.MemberName);
                    array_size = int64Array.Length;
                    tmp_bytes = new byte[array_size * element_size];
                    Buffer.BlockCopy(int64Array, 0, tmp_bytes, 0, tmp_bytes.Length);
                    allocator.Add(tmp_bytes, parent_off + elm.ByteMemberOffset, tmp_bytes.Length);
                    return array_size;
                case "uint8":
                    byte[] uint8Array = src.GetDataArray<Byte>(elm.MemberName);
                    array_size = uint8Array.Length;
                    allocator.Add(uint8Array, parent_off + elm.ByteMemberOffset, array_size * element_size);
                    //Debug.Log("uint8: parent_off: " + parent_off + " elm.offset:" + elm.offset + " array_size:" + array_size);
                    return array_size;
                case "uint16":
                    ushort[] uint16Array = src.GetDataArray<UInt16>(elm.MemberName);
                    array_size = uint16Array.Length;
                    tmp_bytes = new byte[array_size * element_size];
                    Buffer.BlockCopy(uint16Array, 0, tmp_bytes, 0, tmp_bytes.Length);
                    allocator.Add(tmp_bytes, parent_off + elm.ByteMemberOffset, tmp_bytes.Length);
                    return array_size;
                case "uint32":
                    uint[] uint32Array = src.GetDataArray<UInt32>(elm.MemberName);
                    array_size = uint32Array.Length;
                    tmp_bytes = new byte[array_size * element_size];
                    Buffer.BlockCopy(uint32Array, 0, tmp_bytes, 0, tmp_bytes.Length);
                    allocator.Add(tmp_bytes, parent_off + elm.ByteMemberOffset, tmp_bytes.Length);
                    return array_size;
                case "uint64":
                    ulong[] uint64Array = src.GetDataArray<UInt64>(elm.MemberName);
                    array_size = uint64Array.Length;
                    tmp_bytes = new byte[array_size * element_size];
                    Buffer.BlockCopy(uint64Array, 0, tmp_bytes, 0, tmp_bytes.Length);
                    allocator.Add(tmp_bytes, parent_off + elm.ByteMemberOffset, tmp_bytes.Length);
                    return array_size;
                case "float32":
                    float[] float32Array = src.GetDataArray<float>(elm.MemberName);
                    array_size = float32Array.Length;
                    tmp_bytes = new byte[array_size * element_size];
                    Buffer.BlockCopy(float32Array, 0, tmp_bytes, 0, tmp_bytes.Length);
                    allocator.Add(tmp_bytes, parent_off + elm.ByteMemberOffset, tmp_bytes.Length);
                    return array_size;
                case "float64":
                    double[] float64Array = src.GetDataArray<double>(elm.MemberName);
                    array_size = float64Array.Length;
                    tmp_bytes = new byte[array_size * element_size];
                    Buffer.BlockCopy(float64Array, 0, tmp_bytes, 0, tmp_bytes.Length);
                    allocator.Add(tmp_bytes, parent_off + elm.ByteMemberOffset, tmp_bytes.Length);
                    return array_size;
                case "bool":
                    bool[] boolArray = src.GetDataArray<bool>(elm.MemberName);
                    array_size = boolArray.Length;
                    tmp_bytes = new byte[array_size * 4]; // 4バイト長のbool型データ用
                    for (int i = 0; i < array_size; i++)
                    {
                        byte[] boolBytes = new byte[4];
                        boolBytes[0] = boolArray[i] ? (byte)1 : (byte)0;
                        Buffer.BlockCopy(boolBytes, 0, tmp_bytes, i * 4, 4);
                    }
                    allocator.Add(tmp_bytes, parent_off + elm.ByteMemberOffset, tmp_bytes.Length);
                    return array_size;
                case "string":
                    string[] stringArray = src.GetDataArray<string>(elm.MemberName);
                    array_size = stringArray.Length;
                    for (int i = 0; i < array_size; i++)
                    {
                        byte[] stringBytes = Encoding.ASCII.GetBytes(stringArray[i]);
                        byte[] paddedStringBytes = new byte[elm.ByteMemberDataTypeSize];
                        Buffer.BlockCopy(stringBytes, 0, paddedStringBytes, 0, stringBytes.Length);
                        allocator.Add(paddedStringBytes, parent_off + elm.ByteMemberOffset + i * element_size, paddedStringBytes.Length);
                    }
                    return array_size;
                default:
                    throw new InvalidCastException("Error: Cannot find ptype: " + elm.DataTypeName);
            }
        }


        private void ConvertFromPrimtive(int parent_off, PduFieldDefinition elm, DynamicAllocator allocator, IPdu src)
        {
            byte[] tmp_bytes = null;
            switch (elm.DataTypeName)
            {
                case "int8":
                    sbyte sint8v = src.GetData<SByte>(elm.MemberName);
                    tmp_bytes = new byte[] { (byte)sint8v };
                    break;
                case "int16":
                    tmp_bytes = BitConverter.GetBytes(src.GetData<Int16>(elm.MemberName));
                    break;
                case "int32":
                    tmp_bytes = BitConverter.GetBytes(src.GetData<Int32>(elm.MemberName));
                    break;
                case "int64":
                    tmp_bytes = BitConverter.GetBytes(src.GetData<Int64>(elm.MemberName));
                    break;
                case "uint8":
                    var uint8v = src.GetData<Byte>(elm.MemberName);
                    tmp_bytes = new byte[] { uint8v };
                    break;
                case "uint16":
                    tmp_bytes = BitConverter.GetBytes(src.GetData<UInt16>(elm.MemberName));
                    break;
                case "uint32":
                    tmp_bytes = BitConverter.GetBytes(src.GetData<UInt32>(elm.MemberName));
                    break;
                case "uint64":
                    tmp_bytes = BitConverter.GetBytes(src.GetData<UInt64>(elm.MemberName));
                    break;
                case "float32":
                    tmp_bytes = BitConverter.GetBytes(src.GetData<float>(elm.MemberName));
                    break;
                case "float64":
                    tmp_bytes = BitConverter.GetBytes(src.GetData<double>(elm.MemberName));
                    break;
                case "bool":
                    // bool型を4バイトにパディングする
                    tmp_bytes = new byte[4];
                    tmp_bytes[0] = src.GetData<bool>(elm.MemberName) ? (byte)1 : (byte)0;
                    break;
                case "string":
                    tmp_bytes = new byte[elm.ByteMemberDataTypeSize];
                    var str_bytes = System.Text.Encoding.ASCII.GetBytes(src.GetData<string>(elm.MemberName));
                    Array.Copy(str_bytes, tmp_bytes, str_bytes.Length);
                    break;
                default:
                    throw new InvalidCastException("Error: Can not found ptype: " + elm.DataTypeName);
            }
            allocator.Add(tmp_bytes, parent_off + elm.ByteMemberOffset, tmp_bytes.Length);
        }
    }
}
