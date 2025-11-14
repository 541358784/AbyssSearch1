using System;
using UnityEngine;

[Flags]
public enum ColliderTargetType
{
    None = 0,
    PlayerBullet = 1 << 0,
    Player = 1 << 1,
    Enemy = 1 << 2,
    EnemyBullet = 1 << 3,
    Wall = 1 << 4,
}

[Flags]
public enum ColliderShapeType
{
    None = 0,
    Round = 1 << 0,
    Rect = 1 << 1,
}

public struct ColliderShapeData
{
    public float Radius;//圆
    public float Width;//矩形
    public float Height;//矩形
    // public Vector2 Offset;//通用
}