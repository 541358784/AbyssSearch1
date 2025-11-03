using System.Collections.Generic;
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
                _poolRoot = new GameObject("BulletPool");
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
            return bullet;   
        }
        var newBullet = new GameObject("Bullet").AddComponent<NormalBullet>();
        newBullet.transform.SetParent(PoolRoot.transform,false);
        return newBullet;
    }

    public void RecycleBullet(BulletBase bullet)
    {
        bullet.transform.SetParent(PoolRoot.transform,false);
        NormalBulletPool.Push((NormalBullet)bullet);
    }
}