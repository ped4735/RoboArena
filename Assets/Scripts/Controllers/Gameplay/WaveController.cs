using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;


[System.Serializable]
public struct Spawn {

    public bool randomPos;
    [HideIf("randomPos")]
    public Transform spawnPoint;
    public PoolTypes enemyPool;

}

[System.Serializable]
public struct Waves
{
    public int ID;
    public int difficultLevel;
    public List<Spawn> enemys;
}

public class WaveController : MonoBehaviour
{
    [HideInInspector]
    public static WaveController instance;

    public float timeBtwWaves = 5f;
    public Transform spawnPointsRef;
    public List<Waves> waves = new List<Waves>();
    

    private int enemyNumberInWave;
    private List<Transform> spawnPointsRandom;
    private Waves currentWave;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            return;
        }

        spawnPointsRandom = new List<Transform> (spawnPointsRef.GetComponentsInChildren<Transform>());
    }


    [Button(Name = "Spawn Wave Enemy ID")]
    public void SpawnWavesById(int id)
    {
        for (int i = 0; i < waves.Count; i++)
        {
            if(waves[i].ID == id)
            {
                
                for (int j = 0; j < waves[i].enemys.Count; j++)
                {
                    GameObject enemy = PoolController.instance.GetEnemy(waves[i].enemys[j].enemyPool);

                    if(!waves[i].enemys[j].randomPos)
                        enemy.transform.position = waves[i].enemys[j].spawnPoint.position;
                    else
                        enemy.transform.position = spawnPointsRandom[Random.Range(0, spawnPointsRandom.Count)].position;
                }
            }
        }
    }

    public void AddEnemyCount()
    {
        enemyNumberInWave++;
    }

    public void RemoveEnemyCount()
    {
        enemyNumberInWave--;

        if(enemyNumberInWave <= 0)
        {
            Debug.Log("End Wave");
        }
    }

}
