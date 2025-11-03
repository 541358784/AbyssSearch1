
namespace Storage.StorageGenerateClass
{
    /// <summary>
    /// 自动生成的类，基于 LevelData 类型
    /// </summary>
    public class StorageLevelData : StorageClass
    {
        // 属性

        public int LevelId
        {
            get => GetValue<StorageInt>("LevelId").Value;
            set => GetValue<StorageInt>("LevelId").Value = value;
        }

        public float ProgressRate
        {
            get => GetValue<StorageFloat>("ProgressRate").Value;
            set => GetValue<StorageFloat>("ProgressRate").Value = value;
        }

        public StorageList<StorageEnemy> DestroyEnemyList
        {
            get => GetValue<StorageList<StorageEnemy>>("DestroyEnemyList");
            set => SetValue("DestroyEnemyList", value);
        }

    }
}