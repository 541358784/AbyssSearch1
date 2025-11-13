public class EnemyNormal:EnemyBase
{
    public override EnemyType Type => EnemyType.Normal;
    public override float GetCollideAreaRadius()
    {
        return 10f;
    }
}