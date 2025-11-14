using UnityEngine;

public abstract class GameObj:MonoBehaviour,ICollider,IPauseAble
{
    public Vector3 LocalPosition
    {
        get => transform.localPosition; 
        set
        {
            transform.localPosition = value;
            ((ICollider)this).OnColliderPositionChange();
        }
    }

    public Vector3 LastFrameLocalPosition;
    
    public static int StaticGameObjId = 0;
    public int GameObjId;

    public virtual void Awake()
    {
        GameObjId = StaticGameObjId;
        StaticGameObjId++;
    }
    
    public Vector2 GetColliderPosition()
    {
        return LocalPosition;
    }
    public Vector2 GetLastFrameColliderPosition()
    {
        return LastFrameLocalPosition;
    }
    public abstract float GetCollideAreaRadius();//碰撞体半径
    public abstract ColliderTargetType GetColliderTargetType();//碰撞体类型
    public abstract ColliderShapeType GetColliderShapeType();//碰撞体形状类型
    public abstract ColliderShapeData GetCollideShapeData();//碰撞体形状数据

    public virtual void LogicUpdate()//Update
    {
        LastFrameLocalPosition = LocalPosition;//更新开始时记录位置
    }
    
}