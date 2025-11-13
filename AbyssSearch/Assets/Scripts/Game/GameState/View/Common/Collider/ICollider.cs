using System;
using System.Collections.Generic;
using UnityEngine;

public interface ICollider
{
    public Vector2 GetColliderPosition();
    public Vector2 ColliderPosition => GetColliderPosition();
    public void OnCollide(ICollider collider);
    public ColliderTargetType GetColliderTargetType();
    public ColliderTargetType Type => GetColliderTargetType();
    public ColliderShapeType GetColliderShapeType();
    public ColliderShapeType ShapeType => GetColliderShapeType();
    
    public ColliderShapeData GetCollideShapeData();
    public ColliderShapeData ShapeData => GetCollideShapeData();
    public void OnColliderPositionChange()//碰撞体位置变化事件
    {
        EventManager.Instance.SendEventImmediately(new EventColliderPositionChange(this));
    }
    public void ColliderRegister()//注册碰撞体
    {
        EventManager.Instance.SendEventImmediately(new EventColliderRegister(this));
    }
    public void ColliderUnRegister()//注销碰撞体
    {
        EventManager.Instance.SendEventImmediately(new EventColliderUnRegister(this));
    }
}
