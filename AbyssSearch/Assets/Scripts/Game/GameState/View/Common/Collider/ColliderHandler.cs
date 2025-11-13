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
    }

    public void PlayerBullet2Enemy(ICollider a,ICollider b)
    {
        
    }
    public void EnemyBullet2Player(ICollider a,ICollider b)
    {
        
    }
    public void Enemy2Player(ICollider a,ICollider b)
    {
        
    }
}