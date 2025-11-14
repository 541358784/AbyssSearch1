using System;
using System.Collections.Generic;
using UnityEngine;

public interface ICollider
{
    public Vector2 GetColliderPosition();
    public Vector2 ColliderPosition => GetColliderPosition();
    public Vector2 LastFrameColliderPosition => GetLastFrameColliderPosition();//上一帧的碰撞体位置，用来计算撞墙后的位置
    public Vector2 GetLastFrameColliderPosition();
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

    public Rect ColliderSurroundRect//最小包围矩形
    {
        get
        {
            if (ShapeType == ColliderShapeType.Rect)
            {
                return new Rect(
                    ColliderPosition.x - ShapeData.Width / 2,
                    ColliderPosition.y - ShapeData.Height / 2,
                    ShapeData.Width,
                    ShapeData.Height);
            }
            else if (ShapeType == ColliderShapeType.Round)
            {
                return new Rect(
                    ColliderPosition.x - ShapeData.Radius,
                    ColliderPosition.y - ShapeData.Radius,
                    ShapeData.Radius*2,
                    ShapeData.Radius*2);
            }
            return Rect.zero;
        }
    }
}
