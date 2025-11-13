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
    
    public static int StaticGameObjId = 0;
    public int GameObjId;

    public virtual void Awake()
    {
        GameObjId = StaticGameObjId;
        StaticGameObjId++;
    }
    
    public Vector3 GetColliderPosition()
    {
        return LocalPosition;
    }
    public abstract float GetCollideAreaRadius();//碰撞体半径
    public abstract void OnCollide(ICollider collider);//碰撞处理
    public abstract ColliderTargetType GetColliderTargetType();//碰撞体类型
    public abstract void LogicUpdate();//Update
}