using System;
using System.Collections.Generic;
using hakoniwa.pdu.interfaces;

namespace hakoniwa.pdu.core
{
    public class Pdu : IPduOperation
    {
        private Dictionary<string, object> fields = new Dictionary<string, object>();

        public string Name { get; }
        public string TypeName { get; }
        public string PackageName { get; }

        public Pdu(string name, string typeName, string packageName)
        {
            this.Name = name;
            this.TypeName = typeName;
            this.PackageName = packageName;
        }

        // データをセット（初期設定か型チェック）
        public void SetData<T>(string field_name, T value)
        {
            if (!fields.ContainsKey(field_name))
            {
                // 初回呼び出しの場合、フィールドを新規追加
                fields[field_name] = value;
            }
            else
            {
                // 2回目以降は型の整合性をチェック
                ValidateType(field_name, value);
                fields[field_name] = value;
            }
        }

        // 配列データをセット（初期設定か型チェック）
        public void SetData<T>(string field_name, T[] value)
        {
            if (!fields.ContainsKey(field_name))
            {
                fields[field_name] = value.Clone(); // 初回呼び出しでフィールドを新規追加
            }
            else
            {
                ValidateType(field_name, value);
                fields[field_name] = value.Clone(); // 2回目以降は型をチェックしてから更新
            }
        }

        // データの取得（存在確認と型チェック）
        public T GetData<T>(string field_name)
        {
            ValidateFieldExists(field_name);

            if (fields[field_name] is T value)
            {
                return value;
            }
            throw new InvalidCastException($"Field '{field_name}' does not contain data of type {typeof(T)}.");
        }

        // 配列データの取得（存在確認と型チェック）
        public T[] GetDataArray<T>(string field_name)
        {
            ValidateFieldExists(field_name);

            if (fields[field_name] is T[] array)
            {
                return (T[])array.Clone();
            }
            throw new InvalidCastException($"Field '{field_name}' does not contain an array of type {typeof(T)}.");
        }

        // バリデーション：フィールドが存在するか確認
        private void ValidateFieldExists(string field_name)
        {
            if (string.IsNullOrEmpty(field_name))
            {
                throw new ArgumentException("Field name cannot be null or empty.");
            }

            if (!fields.ContainsKey(field_name))
            {
                throw new KeyNotFoundException($"Field '{field_name}' does not exist in the PDU.");
            }
        }

        // バリデーション：既存の型と一致するか確認
        private void ValidateType<T>(string field_name, T value)
        {
            Type existingType = fields[field_name].GetType();
            Type newType = value.GetType();

            if (existingType != newType)
            {
                throw new InvalidOperationException(
                    $"Cannot set field '{field_name}' with a different data type. " +
                    $"Existing type: {existingType}, new type: {newType}.");
            }
        }
    }
}
