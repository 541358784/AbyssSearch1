using UnityEngine;

public abstract class BulletBase:GameObj
{
    public abstract void Init(BulletInitDataStruct bulletData);

    public virtual float Damage()
    {
        return 0;
    }

    public virtual void Release()
    {
        BulletFactory.Instance.RecycleBullet(this);//子弹撞墙直接回收
    }
}