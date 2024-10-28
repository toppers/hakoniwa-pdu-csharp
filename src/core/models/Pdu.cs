using System;
using System.Collections.Generic;
using hakoniwa_pdu_csharp.core.interfaces;
public class Pdu : IPdu, IPduOperation
{
    private string pdu_type_name;
    private string my_package_name;

    private Dictionary<string, object> fields = new Dictionary<string, object>();
    private Dictionary<string, object> initialValues = new Dictionary<string, object>();

    public Pdu(string pduTypeName, string packageName = null)
    {
        this.pdu_type_name = pduTypeName;
        this.my_package_name = packageName;
    }

    public string Name => this.pdu_type_name;
    public string PackageName => this.my_package_name;

    // SetData メソッド (初期値を保存)
    public void SetData<T>(string field_name, T value)
    {
        fields[field_name] = value;
        if (!initialValues.ContainsKey(field_name))
        {
            initialValues[field_name] = value;
        }
    }

    public void SetData<T>(string field_name, T[] value)
    {
        fields[field_name] = value;
        if (!initialValues.ContainsKey(field_name))
        {
            initialValues[field_name] = (T[])value.Clone();
        }
    }

    public void SetData<T>(string field_name, int off, T value)
    {
        if (fields.ContainsKey(field_name) && fields[field_name] is T[] array)
        {
            array[off] = value;
        }
        else
        {
            throw new ArgumentException($"Invalid PDU array access: field_name={field_name}");
        }
    }

    public void SetData(string field_name, IPdu pdu)
    {
        fields[field_name] = pdu;
        if (!initialValues.ContainsKey(field_name))
        {
            initialValues[field_name] = pdu;
        }
    }

    public void SetData(string field_name, IPdu[] pduArray)
    {
        fields[field_name] = pduArray;
        if (!initialValues.ContainsKey(field_name))
        {
            initialValues[field_name] = (IPdu[])pduArray.Clone();
        }
    }

    public void SetData(string field_name, int off, IPdu pdu)
    {
        if (fields.ContainsKey(field_name) && fields[field_name] is IPdu[] array)
        {
            array[off] = pdu;
        }
        else
        {
            throw new ArgumentException($"Invalid PDU array access: field_name={field_name}");
        }
    }

    public void InitializePduArray(string field_name, int array_size)
    {
        IPdu[] newArray = new IPdu[array_size];
        for (int i = 0; i < array_size; i++)
        {
            newArray[i] = new Pdu(pdu_type_name);
        }
        fields[field_name] = newArray;

        if (!initialValues.ContainsKey(field_name))
        {
            initialValues[field_name] = (IPdu[])newArray.Clone();
        }
    }

    public T GetData<T>(string field_name)
    {
        if (fields.ContainsKey(field_name) && fields[field_name] is T value)
        {
            return value;
        }
        throw new ArgumentException($"Invalid PDU access: field_name={field_name}");
    }

    public T[] GetDataArray<T>(string field_name)
    {
        if (fields.ContainsKey(field_name) && fields[field_name] is T[] array)
        {
            return array;
        }
        throw new ArgumentException($"Invalid PDU access: field_name={field_name}");
    }

    public IPdu Ref(string field_name)
    {
        if (fields.ContainsKey(field_name) && fields[field_name] is IPdu pdu)
        {
            return pdu;
        }
        throw new ArgumentException($"Invalid PDU reference: field_name={field_name}");
    }

    public IPdu[] Refs(string field_name)
    {
        if (fields.ContainsKey(field_name) && fields[field_name] is IPdu[] array)
        {
            return array;
        }
        throw new ArgumentException($"Invalid PDU reference array: field_name={field_name}");
    }

    public void Reset()
    {
        foreach (var field in initialValues)
        {
            if (field.Value is Array array)
            {
                fields[field.Key] = ((Array)field.Value).Clone();
            }
            else
            {
                fields[field.Key] = field.Value;
            }
        }
    }
}
