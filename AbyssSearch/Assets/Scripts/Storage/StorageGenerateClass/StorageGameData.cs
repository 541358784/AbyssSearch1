
namespace Storage.StorageGenerateClass
{
    /// <summary>
    /// 自动生成的类，基于 GameData 类型
    /// </summary>
    public class StorageGameData : StorageClass
    {
        // 属性

        public StoragePlayerData PlayerData
        {
            get => GetValue<StoragePlayerData>("PlayerData");
            set => SetValue("PlayerData", value);
        }

        public StorageLevelData LevelData
        {
            get => GetValue<StorageLevelData>("LevelData");
            set => SetValue("LevelData", value);
        }

    }
}