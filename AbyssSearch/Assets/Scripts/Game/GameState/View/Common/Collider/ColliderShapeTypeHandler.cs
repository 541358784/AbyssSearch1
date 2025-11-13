using System;
using System.Collections.Generic;
using UnityEngine;

public class ColliderShapeHandler:Singleton<ColliderShapeHandler>
{
    Dictionary<ColliderShapeType, Func<ICollider, ICollider,bool>> handlers = new();
    public Dictionary<ColliderShapeType, ColliderShapeType> ColliderShapeTypeTriggerMap = new();
    
    
    
    public void Register(ColliderShapeType a, ColliderShapeType b,
        Func<ICollider, ICollider,bool> handler)
    {
        handlers[a|b] = handler;
        if (!ColliderShapeTypeTriggerMap.TryAdd(a,b))
            ColliderShapeTypeTriggerMap[a] |= b;
        if (!ColliderShapeTypeTriggerMap.TryAdd(b,a))
            ColliderShapeTypeTriggerMap[b] |= a;
    }

    public ColliderShapeType GetTriggerTypes(ColliderShapeType type)
    {
        if (ColliderShapeTypeTriggerMap.TryGetValue(type,out var triggerTypes))
            return triggerTypes;
        return ColliderShapeType.None;
    }
    public void Resolve(ICollider a, ICollider b)
    {
        if (handlers.TryGetValue(a.ShapeType | b.ShapeType, out var h))
            h(a, b);
    }
    public void OnColliderTrigger(EventColliderTrigger evt)
    {
        Resolve(evt.A, evt.B);//处理碰撞事件
    }
    public ColliderShapeHandler()
    {
        Init();
        EventManager.Instance.AddEvent<EventColliderTrigger>(OnColliderTrigger);//监听碰撞事件
    }
    
    public void Init()
    {
        Register(ColliderShapeType.Round, ColliderShapeType.Round, Round2Round);
        Register(ColliderShapeType.Round, ColliderShapeType.Rect, Round2Rect);
        Register(ColliderShapeType.Rect, ColliderShapeType.Rect, Rect2Rect);
    }

    public bool Round2Round(ICollider a,ICollider b)
    {
        var distance = a.ColliderPosition - b.ColliderPosition;
        var distanceValue = distance.magnitude;
        var radius = a.ShapeData.Radius + b.ShapeData.Radius;
        if (distanceValue <= radius) //圆形碰撞体判断
        {
            return true;
        }
        return false;
    }
    public bool Round2Rect(ICollider a,ICollider b)
    {
        var round = a.ShapeType == ColliderShapeType.Round ? a : b;
        var rect = a.ShapeType == ColliderShapeType.Rect ? a : b;
        Vector2 closestPoint = new Vector2(
            Mathf.Clamp(round.ColliderPosition.x, rect.ColliderPosition.x - rect.ShapeData.Width/2, rect.ColliderPosition.x + rect.ShapeData.Width/2),
            Mathf.Clamp(round.ColliderPosition.y, rect.ColliderPosition.y - rect.ShapeData.Height/2, rect.ColliderPosition.y + rect.ShapeData.Height/2)
        );
        Vector2 direction = round.ColliderPosition - closestPoint;
        float distanceValue = direction.magnitude;
    
        // 3. 检测碰撞
        if (distanceValue <= round.ShapeData.Radius)
        {
            return true;
        }
        return false;
    }
    public bool Rect2Rect(ICollider a,ICollider b)
    {
        bool isColliding = a.Max.x > b.Min.x && a.Min.x < b.Max.x &&
                           a.Max.y > b.Min.y && a.Min.y < b.Max.y;
    }
}