namespace hakoniwa_pdu_csharp.core.interfaces
{
    public interface IPduOperation
    {
        // Set data for a single value or array
        void SetData<T>(string field_name, T value);
        void SetData<T>(string field_name, T[] value);
        void SetData<T>(string field_name, int off, T value);

        // Set nested PDU data
        void SetData(string field_name, IPdu pdu);
        void SetData(string field_name, IPdu[] pduArray);
        void SetData(string field_name, int off, IPdu pdu);

        // Initialize a PDU array with a specific size
        void InitializePduArray(string field_name, int array_size);

        // Get data as single value or array
        T GetData<T>(string field_name);
        T[] GetDataArray<T>(string field_name);

        // Retrieve nested PDU references
        IPdu Ref(string field_name);
        IPdu[] Refs(string field_name);
    }
}
