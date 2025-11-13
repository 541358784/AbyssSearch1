using System;

[Flags]
public enum ColliderType
{
    None = 0,
    PlayerBullet = 1 << 0,
    Player = 1 << 1,
    Enemy = 1 << 2,
    EnemyBullet = 1 << 3,
}