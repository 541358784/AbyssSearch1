using System;
using System.Collections.Generic;
using Storage.StorageGenerateClass;
using UnityEngine;

public class Player : GameObj
{
    public override ColliderType GetColliderType()
    {
        return ColliderType.Player;
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
        return state;
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
        var speedChange = new Dictionary<ControlDirection, float>();
        
        
        
        
        
        var moveDistance = new Vector2(0,0);
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