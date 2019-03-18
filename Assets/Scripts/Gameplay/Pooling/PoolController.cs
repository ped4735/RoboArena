using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[System.Serializable]
public struct PoolDB
{
    public PoolTypes poolType;
    public Pool pool;
}


public class PoolController : MonoBehaviour
{

    public static PoolController instance;

    public List<PoolDB> enemiesPool = new List<PoolDB>();
    public List<PoolDB> bulletsPool = new List<PoolDB>();
    public List<PoolDB> vfxPool = new List<PoolDB>();

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    public GameObject GetBullet(PoolTypes type)
    {
        for (int i = 0; i < bulletsPool.Count; i++)
        {
            if(bulletsPool[i].poolType == type)
            {
                return bulletsPool[i].pool.nextThing;
            }
        }

        return null;
        
    }

    public GameObject GetEnemy(PoolTypes type)
    {
        for (int i = 0; i < enemiesPool.Count; i++)
        {
            if (enemiesPool[i].poolType == type)
            {
                return enemiesPool[i].pool.nextThing;
            }
        }

        return null;

    }

    public GameObject GetVFX(PoolTypes type)
    {
        for (int i = 0; i < vfxPool.Count; i++)
        {
            if (vfxPool[i].poolType == type)
            {
                return vfxPool[i].pool.nextThing;
            }
        }

        return null;

    }


    [Button(Name = "Desativar todos os Inimigos")]
    public void DeactivetaAllEnemys()
    {
        for (int i = 0; i < enemiesPool.Count; i++)
        {
            EnemyDamage[] enemys = enemiesPool[i].pool.GetComponentsInChildren<EnemyDamage>();

            for (int j = 0; j < enemys.Length; j++)
            {
                enemys[j].Death();
            }
        }   
    }

}
