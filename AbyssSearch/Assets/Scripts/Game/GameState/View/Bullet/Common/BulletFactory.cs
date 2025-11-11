using System.Collections.Generic;
using Framework;
using Unity.VisualScripting;
using UnityEngine;

public class BulletFactory:Singleton<BulletFactory>
{
    private List<NormalBullet> NormalBulletPool = new List<NormalBullet>();

    private GameObject _poolRoot;
    public GameObject PoolRoot
    {
        get
        {
            if (_poolRoot == null)
            {
                _poolRoot = GameObjectFactory.Create(false,"BulletPool");
                _poolRoot.SetActive(false);
            }
            return _poolRoot;
        }
    }
    public BulletBase CreateBullet()
    {
        if (NormalBulletPool.Count > 0)
        {
            var bullet = NormalBulletPool.Pop();
            bullet.Reset();
            bullet.ColliderRegister();
            return bullet;   
        }
        var newBullet = new GameObject("Bullet").AddComponent<NormalBullet>();
        newBullet.transform.SetParent(PoolRoot.transform,false);
        newBullet.ColliderRegister();
        return newBullet;
    }

    public void RecycleBullet(BulletBase bullet)
    {
        bullet.transform.SetParent(PoolRoot.transform,false);
        bullet.ColliderUnRegister();
        NormalBulletPool.Push((NormalBullet)bullet);
    }
}