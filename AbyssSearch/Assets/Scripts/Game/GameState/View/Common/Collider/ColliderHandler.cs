using System;
using System.Collections.Generic;

public class ColliderHandler:Singleton<ColliderHandler>
{ 
    Dictionary<ColliderTargetType, Action<ICollider, ICollider>> handlers = new();
    public Dictionary<ColliderTargetType, ColliderTargetType> ColliderTargetTypeTriggerMap = new();
    public void Register(ColliderTargetType a, ColliderTargetType b,
        Action<ICollider, ICollider> handler)
    {
        handlers[a|b] = handler;
        if (!ColliderTargetTypeTriggerMap.TryAdd(a,b))
            ColliderTargetTypeTriggerMap[a] |= b;
        if (!ColliderTargetTypeTriggerMap.TryAdd(b,a))
            ColliderTargetTypeTriggerMap[b] |= a;
    }

    public ColliderTargetType GetTriggerTypes(ColliderTargetType type)
    {
        if (ColliderTargetTypeTriggerMap.TryGetValue(type,out var triggerTypes))
            return triggerTypes;
        return ColliderTargetType.None;
    }
    public void Resolve(ICollider a, ICollider b)
    {
        if (handlers.TryGetValue(a.Type | b.Type, out var h))
            h(a, b);
    }
    public void OnColliderTrigger(EventColliderTrigger evt)
    {
        Resolve(evt.A, evt.B);//处理碰撞事件
    }
    public ColliderHandler()
    {
        Init();
        EventManager.Instance.AddEvent<EventColliderTrigger>(OnColliderTrigger);//监听碰撞事件
    }
    
    public void Init()
    {
        Register(ColliderTargetType.PlayerBullet, ColliderTargetType.Enemy, PlayerBullet2Enemy);
        Register(ColliderTargetType.EnemyBullet, ColliderTargetType.Player, EnemyBullet2Player);
        Register(ColliderTargetType.Enemy, ColliderTargetType.Player, Enemy2Player);
        Register(ColliderTargetType.Wall, ColliderTargetType.Player, Wall2Player);
        Register(ColliderTargetType.Wall, ColliderTargetType.Enemy, Wall2Enemy);
        Register(ColliderTargetType.Wall, ColliderTargetType.PlayerBullet, Wall2PlayerBullet);
        Register(ColliderTargetType.Wall, ColliderTargetType.EnemyBullet, Wall2EnemyBullet);
        
    }

    public void PlayerBullet2Enemy(ICollider a,ICollider b)
    {
        var bullet = a.Type == ColliderTargetType.PlayerBullet ? a : b;
        var target = a.Type == ColliderTargetType.Enemy ? a : b;
        var damage = ((BulletBase)bullet).Damage();
        
        
    }
    public void EnemyBullet2Player(ICollider a,ICollider b)
    {
        
    }
    public void Enemy2Player(ICollider a,ICollider b)
    {
        
    }

    private const int DichotomyTimes = 5;
    public void RejectOverlap(ICollider a,ICollider b)//拒绝重叠算法
    {
        var aMin = a.LastFrameColliderPosition;
        var aMax = a.ColliderPosition;
        var bMin = b.LastFrameColliderPosition;
        var bMax = b.ColliderPosition;
        var progressBottom = 0f;
        var progressTop = 1f;
        for (var i = 0; i < DichotomyTimes; i++)
        {
            var mid = (progressBottom + progressTop) / 2;
            ((GameObj)a).LocalPosition = aMin + (aMax - aMin) * mid;
            ((GameObj)b).LocalPosition = bMin + (bMax - bMin) * mid;
            var isOverLap = ColliderShapeHandler.Instance.IsOverlap(a, b);
            if (isOverLap)
            {
                progressTop = mid;
            }
            else
            {
                progressBottom = mid;
            }
        }
    }
    public void Wall2Player(ICollider a,ICollider b)
    {
        RejectOverlap(a, b);
    }
    public void Wall2Enemy(ICollider a,ICollider b)
    {
        RejectOverlap(a, b);
    }
    public void Wall2PlayerBullet(ICollider a,ICollider b)
    {
        var bullet = a.Type == ColliderTargetType.PlayerBullet ? a : b;
        ((BulletBase)bullet).Release();//子弹撞墙直接回收
    }
    public void Wall2EnemyBullet(ICollider a,ICollider b)
    {
        var bullet = a.Type == ColliderTargetType.EnemyBullet ? a : b;
        ((BulletBase)bullet).Release();//子弹撞墙直接回收
    }
}