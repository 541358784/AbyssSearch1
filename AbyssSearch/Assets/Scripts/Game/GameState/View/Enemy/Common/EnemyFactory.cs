using System.Collections.Generic;
using Framework;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyFactory:Singleton<EnemyFactory>
{
    private Dictionary<EnemyType,List<EnemyBase>> EnemyPoolDic = new();

    public void Init()
    {
        
    }
    
    private GameObject _poolRoot;
    public GameObject PoolRoot
    {
        get
        {
            if (_poolRoot == null)
            {
                _poolRoot = GameObjectFactory.Create(false,"EnemyPool");
                _poolRoot.SetActive(false);
            }
            return _poolRoot;
        }
    }
    public EnemyBase CreateEnemy(EnemyType enemyType)
    {
        if (!EnemyPoolDic.TryGetValue(enemyType, out var enemyPool))
        {
            enemyPool = new List<EnemyBase>();
            EnemyPoolDic.Add(enemyType,enemyPool);
        }
        if (enemyPool.Count > 0)
        {
            var enemy = enemyPool.Pop();
            enemy.Reset();
            ((ICollider)enemy).ColliderRegister();
            ((IPauseAble)enemy).PauseAbleRegister();
            return enemy;   
        }

        EnemyBase newEnemy = null;
        if (enemyType == EnemyType.Normal)
        {
            newEnemy = new GameObject("Enemy").AddComponent<EnemyNormal>();
        }
        newEnemy.transform.SetParent(PoolRoot.transform,false);
        ((ICollider)newEnemy).ColliderRegister();
        ((IPauseAble)newEnemy).PauseAbleRegister();
        return newEnemy;
    }

    public void RecycleEnemy(EnemyBase enemy)
    {
        enemy.transform.SetParent(PoolRoot.transform,false);
        ((ICollider)enemy).ColliderUnRegister();
        ((IPauseAble)enemy).PauseAbleUnRegister();
        EnemyPoolDic[enemy.Type].Push(enemy);
    }
}