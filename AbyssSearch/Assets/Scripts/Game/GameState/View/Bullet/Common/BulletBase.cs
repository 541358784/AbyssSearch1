using UnityEngine;

public abstract class BulletBase:Collider,IPauseAble
{
    public void Reset()
    {
        transform.localScale = Vector3.one;
        transform.localPosition = Vector3.zero;
        transform.localRotation = new Quaternion(0, 0, 0, 0);
    }

    public abstract void Init(BulletInitDataStruct bulletData);
    public abstract void LogicUpdate();
}