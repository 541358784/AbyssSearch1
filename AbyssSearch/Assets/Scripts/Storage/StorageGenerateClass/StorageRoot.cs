
namespace Storage.StorageGenerateClass
{
    /// <summary>
    /// 自动生成的类，基于 Root 类型
    /// </summary>
    public class StorageRoot : StorageClass
    {
        // 属性

        public StorageUserData Userdata
        {
            get => GetValue<StorageUserData>("Userdata");
            set => SetValue("Userdata", value);
        }

        public StorageGameData GameData
        {
            get => GetValue<StorageGameData>("GameData");
            set => SetValue("GameData", value);
        }

    }
}