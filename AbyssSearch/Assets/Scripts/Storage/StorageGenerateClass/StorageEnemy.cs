
namespace Storage.StorageGenerateClass
{
    /// <summary>
    /// 自动生成的类，基于 Enemy 类型
    /// </summary>
    public class StorageEnemy : StorageClass
    {
        // 属性

        public int EnemyId
        {
            get => GetValue<StorageInt>("EnemyId").Value;
            set => GetValue<StorageInt>("EnemyId").Value = value;
        }

        public float DifficultyValue
        {
            get => GetValue<StorageFloat>("DifficultyValue").Value;
            set => GetValue<StorageFloat>("DifficultyValue").Value = value;
        }

    }
}