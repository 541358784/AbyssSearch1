using UnityEngine;

public abstract class BulletBase:GameObj
{
    public void Reset()
    {
        transform.localScale = Vector3.one;
        transform.localPosition = Vector3.zero;
        transform.localRotation = new Quaternion(0, 0, 0, 0);
    }

    public abstract void Init(BulletInitDataStruct bulletData);

    public virtual int Damage()
    {
        return 0;
    }

    public virtual void Release()
    {
        BulletFactory.Instance.RecycleBullet(this);//子弹撞墙直接回收
    }
}