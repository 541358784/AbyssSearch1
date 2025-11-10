using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class Collider : MonoBehaviour
{
    public Vector3 LocalPosition
    {
        get { return transform.localPosition; }
        set
        {
            transform.localPosition = value;
            OnLocalPositionChange();
        }
    }

    public virtual void OnCollide(Collider collider)
    {
    }

    public static int StaticId = 0;
    public int Id;

    public virtual void Awake()
    {
        Id = StaticId;
        StaticId++;
    }

    public abstract ColliderType GetColliderType();
    public ColliderType Type => GetColliderType();
    public virtual float CollideAreaRadius => 0;

    public void OnLocalPositionChange()
    {
        EventManager.Instance.SendEventImmediately(new EventColliderLocalPositionChange(this));
    }
}
