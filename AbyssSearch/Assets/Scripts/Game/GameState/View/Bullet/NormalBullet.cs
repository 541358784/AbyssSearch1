using UnityEngine;

public class NormalBullet : BulletBase
{
    public float Speed;
    public float Distance;
    public float Damage;
    public Vector2 Direction;
    public float DistancePass;
    public override void Init(BulletInitDataStruct bulletData)
    {
        Speed = bulletData.Speed;
        Distance = bulletData.Distance;
        Damage = bulletData.Damage;
        Direction = bulletData.Direction;
        DistancePass = 0;
    }

    public override void LogicUpdate()
    {
        var time = Time.deltaTime;
        var needRecycle = false;
        var moveDistance = time * Speed;
        DistancePass += moveDistance;
        if (DistancePass >= Distance)
        {
            moveDistance -= DistancePass - Distance;
            DistancePass = Distance;
            needRecycle = true;
        }
        
        LocalPosition += (Vector3)Direction.normalized * moveDistance;
    }

    public override void OnCollide(ICollider collider)
    {
        
    }

    public override float GetCollideAreaRadius()
    {
        return 1f;
    }

    public override ColliderTargetType GetColliderTargetType()
    {
        return ColliderTargetType.PlayerBullet;
    }
}