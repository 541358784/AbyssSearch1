using System;
using Storage.StorageGenerateClass;
using UnityEngine;

public class Player : Collider,IPauseAble
{
    public override ColliderType GetColliderType()
    {
        return ColliderType.Player;
    }
    public StoragePlayerData Storage => StorageManager.Instance.Root.GameData.PlayerData;

    public void LogicUpdate()
    {
        HandleMove();
        HandleAttack();
    }

    public void HandleMove()
    {
        var up = Input.GetKeyDown(KeyCode.W);
        var down = Input.GetKeyDown(KeyCode.S);
        var left = Input.GetKeyDown(KeyCode.A);
        var right = Input.GetKeyDown(KeyCode.D);
        var moveSpeed = PlayerConfig.MoveSpeed;
        var moveDistance = new Vector2(0,0);
        if (up)
            moveDistance.y += moveSpeed * Time.deltaTime;
        if (down)
            moveDistance.y -= moveSpeed * Time.deltaTime;
        if (left)
            moveDistance.x -= moveSpeed * Time.deltaTime;
        if (right)
            moveDistance.x += moveSpeed * Time.deltaTime;
        transform.localPosition += (Vector3)moveDistance;
    }

    private float lastAttackTime;
    public float AttackIntervalScale = 0.15f;
    public void HandleAttack()
    {
        var interval = PlayerConfig.AttackInterval * AttackIntervalScale;
        var timePass = Time.time - lastAttackTime;
        if (timePass < interval)
        {
            return;
        }
        lastAttackTime += interval;
        var bullet = BulletFactory.Instance.CreateBullet();
        

    }
}