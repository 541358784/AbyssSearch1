using System;
using System.Collections.Generic;
using Storage.StorageGenerateClass;
using UnityEngine;

public class Player : GameObj
{
    public override ColliderTargetType GetColliderTargetType()
    {
        return ColliderTargetType.Player;
    }

    public override void OnCollide(ICollider collider)
    {
        
    }

    public override float GetCollideAreaRadius()
    {
        return 10f;
    }

    public StoragePlayerData Storage => StorageManager.Instance.Root.GameData.PlayerData;
    public override void Awake()
    {
        base.Awake();
        Init();
    }

    public void Init()
    {
        CurrentSpeed = Vector2.zero;
        CurrentControlState = new Dictionary<ControlDirection, bool>();
        foreach ( ControlDirection dir in Enum.GetValues(typeof(ControlDirection)))
        {
            CurrentControlState.Add(dir,false);
        }
    }

    public override void LogicUpdate()
    {
        HandleMove();
        HandleAttack();
    }

    public enum ControlDirection
    {
        Up,
        Down,
        Left,
        Right
    }
    public readonly Dictionary<ControlDirection, KeyCode> ControlKeyMap = new Dictionary<ControlDirection, KeyCode>()
    {
        { ControlDirection.Up, KeyCode.W },
        { ControlDirection.Down, KeyCode.S },
        { ControlDirection.Left, KeyCode.A },
        { ControlDirection.Right, KeyCode.D },
    };
    public Dictionary<ControlDirection, bool> GetControlState()
    {
        var state = new Dictionary<ControlDirection, bool>();
        foreach (var pair in ControlKeyMap)
        {
            state.Add(pair.Key,Input.GetKeyDown(pair.Value));
        }
        DealOppositeDirection(state);
        return state;
    }

    public void DealOppositeDirection(Dictionary<ControlDirection, bool> state)//去掉相对的控制方向，当两个控制方向相对时视为全部无效
    {
        if (state[ControlDirection.Up] && state[ControlDirection.Down])
        {
            state[ControlDirection.Up] = false;
            state[ControlDirection.Down] = false;
        }
        if (state[ControlDirection.Right] && state[ControlDirection.Left])
        {
            state[ControlDirection.Right] = false;
            state[ControlDirection.Left] = false;
        }
    }
    public Dictionary<ControlDirection, bool> CurrentControlState;
    public Vector2 CurrentSpeed;
    public void HandleMove()
    {
        var controlState = GetControlState();
        CurrentControlState = controlState;
        var maxSpeed = PlayerConfig.MoveSpeed;
        var addSpeedPerSec = maxSpeed / PlayerConfig.AddSpeedTime;
        var addSpeed = addSpeedPerSec * Time.deltaTime;
        var reduceSpeedPerSec = maxSpeed/PlayerConfig.ReduceSpeedTime;
        var reduceSpeed = reduceSpeedPerSec * Time.deltaTime;
        var speedChange = Vector2.zero;
        
        if (controlState[ControlDirection.Up])
        {
            speedChange.y += addSpeed;
        }
        else if (CurrentSpeed.y > 0)
        {
            speedChange.y -= reduceSpeed;
        }
        
        if (controlState[ControlDirection.Down])
        {
            speedChange.y -= addSpeed;
        }
        else if (CurrentSpeed.y < 0)
        {
            speedChange.y += reduceSpeed;
        }
        
        if (controlState[ControlDirection.Right])
        {
            speedChange.x += addSpeed;
        }
        else if (CurrentSpeed.x > 0)
        {
            speedChange.x -= reduceSpeed;
        }
        
        if (controlState[ControlDirection.Left])
        {
            speedChange.x -= addSpeed;
        }
        else if (CurrentSpeed.x < 0)
        {
            speedChange.x += reduceSpeed;
        }

        var newSpeed = CurrentSpeed + speedChange;
        if (speedChange.x > 0 && CurrentSpeed.x >= 0)//算速度上限X
        {
            if (newSpeed.x > maxSpeed)
                newSpeed.x = maxSpeed;
        }
        if (speedChange.x < 0 && CurrentSpeed.x <= 0)
        {
            if (newSpeed.x < -maxSpeed)
                newSpeed.x = -maxSpeed;
        }
        if (speedChange.y > 0 && CurrentSpeed.y >= 0)//算速度上限Y
        {
            if (newSpeed.y > maxSpeed)
                newSpeed.y = maxSpeed;
        }
        if (speedChange.y < 0 && CurrentSpeed.y <= 0)
        {
            if (newSpeed.y < -maxSpeed)
                newSpeed.y = -maxSpeed;
        }

        if (speedChange.x > 0 && CurrentSpeed.x <= 0 && !controlState[ControlDirection.Right])//自然减速不超过0X
        {
            if (newSpeed.x > 0)
                newSpeed.x = 0;
        }
        if (speedChange.x < 0 && CurrentSpeed.x >= 0 && !controlState[ControlDirection.Left])
        {
            if (newSpeed.x < 0)
                newSpeed.x = 0;
        }
        if (speedChange.y > 0 && CurrentSpeed.y <= 0 && !controlState[ControlDirection.Up])//自然减速不超过0Y
        {
            if (newSpeed.y > 0)
                newSpeed.y = 0;
        }
        if (speedChange.y < 0 && CurrentSpeed.y >= 0 && !controlState[ControlDirection.Down])
        {
            if (newSpeed.y < 0)
                newSpeed.y = 0;
        }
        CurrentSpeed = newSpeed;
        var moveDistance = CurrentSpeed * Time.deltaTime;
        LocalPosition += (Vector3)moveDistance;
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