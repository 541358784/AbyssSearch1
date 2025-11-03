
namespace Storage.StorageGenerateClass
{
    /// <summary>
    /// 自动生成的类，基于 UserData 类型
    /// </summary>
    public class StorageUserData : StorageClass
    {
        // 属性

        public int UserId
        {
            get => GetValue<StorageInt>("UserId").Value;
            set => GetValue<StorageInt>("UserId").Value = value;
        }

        public int Level
        {
            get => GetValue<StorageInt>("Level").Value;
            set => GetValue<StorageInt>("Level").Value = value;
        }

        public int CoinCount
        {
            get => GetValue<StorageInt>("CoinCount").Value;
            set => GetValue<StorageInt>("CoinCount").Value = value;
        }

        public string Name
        {
            get => GetValue<StorageString>("Name").Value;
            set => GetValue<StorageString>("Name").Value = value;
        }

    }
}