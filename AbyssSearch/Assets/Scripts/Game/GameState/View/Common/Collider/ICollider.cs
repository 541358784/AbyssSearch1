using System;
using System.Collections.Generic;
using UnityEngine;

public interface ICollider
{
    public Vector3 GetColliderPosition();
    public Vector3 ColliderPosition => GetColliderPosition();
    public void OnCollide(ICollider collider);
    public ColliderType GetColliderType();
    public ColliderType Type => GetColliderType();
    public float GetCollideAreaRadius();
    public float CollideAreaRadius => GetCollideAreaRadius();
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
