using Storage.StorageGenerateClass;
using UnityEngine;

public class Enemy: GameObj
{
        public override ColliderTargetType GetColliderTargetType()
    {
        return ColliderTargetType.Player;
    }

    public override float GetCollideAreaRadius()
    {
        return 10f;
    }

    public StorageEnemy Storage;
    public override void Awake()
    {
        base.Awake();
        Init();
    }

    public void Init()
    {
        CurrentSpeed = Vector2.zero;
        
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        HandleMove();
        HandleAttack();
    }
    public Vector2 CurrentSpeed;
    public void HandleMove()
    {
        
        var speedChange = Vector2.zero;
        var newSpeed = CurrentSpeed + speedChange;
        
        CurrentSpeed = newSpeed;
        var moveDistance = CurrentSpeed * Time.deltaTime;
        LocalPosition += (Vector3)moveDistance;
    }
    
    private float lastAttackTime;
    public float AttackIntervalScale = 0.15f;
    public void HandleAttack()
    {
        var interval = 3f * AttackIntervalScale;
        var timePass = Time.time - lastAttackTime;
        if (timePass < interval)
        {
            return;
        }
        lastAttackTime += interval;
        var bullet = BulletFactory.Instance.CreateBullet();
        

    }

    public override ColliderShapeType GetColliderShapeType()
    {
        return ColliderShapeType.Round;
    }
    public override ColliderShapeData GetCollideShapeData()
    {
        return new ColliderShapeData()
        {
            Radius = 1f
        };
    }
}