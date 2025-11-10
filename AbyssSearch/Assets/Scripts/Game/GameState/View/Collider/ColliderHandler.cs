using System;
using System.Collections.Generic;

public class ColliderHandler:Singleton<ColliderHandler>
{ 
    Dictionary<ColliderType, Action<Collider, Collider>> handlers = new();
    public Dictionary<ColliderType, ColliderType> ColliderTypeTriggerMap = new();
    public void Register(ColliderType a, ColliderType b,
        Action<Collider, Collider> handler)
    {
        handlers[a|b] = handler;
        if (!ColliderTypeTriggerMap.TryAdd(a,b))
            ColliderTypeTriggerMap[a] |= b;
        if (!ColliderTypeTriggerMap.TryAdd(b,a))
            ColliderTypeTriggerMap[b] |= a;
    }

    public ColliderType GetTriggerTypes(ColliderType type)
    {
        if (ColliderTypeTriggerMap.TryGetValue(type,out var triggerTypes))
            return triggerTypes;
        return ColliderType.None;
    }
    public void Resolve(Collider a, Collider b)
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
        Register(ColliderType.PlayerBullet, ColliderType.Enemy, PlayerBullet2Enemy);
        Register(ColliderType.EnemyBullet, ColliderType.Player, EnemyBullet2Player);
        Register(ColliderType.Enemy, ColliderType.Player, Enemy2Player);
    }

    public void PlayerBullet2Enemy(Collider a,Collider b)
    {
        
    }
    public void EnemyBullet2Player(Collider a,Collider b)
    {
        
    }
    public void Enemy2Player(Collider a,Collider b)
    {
        
    }
}