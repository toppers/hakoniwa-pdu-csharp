namespace hakoniwa_pdu_csharp.core.interfaces
{
    public interface IPdu : IPduOperation
    {
        // PDU の基本情報
        string Name { get; }
        string PackageName { get; }

        // PDU のフィールドを初期状態に戻す
        void Reset();
    }
}
