
namespace Storage.StorageGenerateClass
{
    /// <summary>
    /// 自动生成的类，基于 PlayerData 类型
    /// </summary>
    public class StoragePlayerData : StorageClass
    {
        // 属性

        public int Life
        {
            get => GetValue<StorageInt>("Life").Value;
            set => GetValue<StorageInt>("Life").Value = value;
        }

        public float TrustValue
        {
            get => GetValue<StorageFloat>("TrustValue").Value;
            set => GetValue<StorageFloat>("TrustValue").Value = value;
        }

        public float AttackPower
        {
            get => GetValue<StorageFloat>("AttackPower").Value;
            set => GetValue<StorageFloat>("AttackPower").Value = value;
        }

        public float AttackFrequency
        {
            get => GetValue<StorageFloat>("AttackFrequency").Value;
            set => GetValue<StorageFloat>("AttackFrequency").Value = value;
        }

        public float MoveSpeed
        {
            get => GetValue<StorageFloat>("MoveSpeed").Value;
            set => GetValue<StorageFloat>("MoveSpeed").Value = value;
        }

        public float AttackRange
        {
            get => GetValue<StorageFloat>("AttackRange").Value;
            set => GetValue<StorageFloat>("AttackRange").Value = value;
        }

        public float AttackSpeed
        {
            get => GetValue<StorageFloat>("AttackSpeed").Value;
            set => GetValue<StorageFloat>("AttackSpeed").Value = value;
        }

    }
}