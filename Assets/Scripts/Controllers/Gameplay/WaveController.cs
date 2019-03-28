using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class WaveController : MonoBehaviour
{
    [HideInInspector]
    public static WaveController instance;
    
    public Transform spawnPointsRef;
    private List<Transform> spawnPoints;
    private List<Waves> wavesByLevel = new List<Waves>();

    private void Awake()
    {

        #region SingleTon
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            return;
        } 
        #endregion

        spawnPoints = new List<Transform> (spawnPointsRef.GetComponentsInChildren<Transform>());
    }
    
    [Button(Name = "Spawn Wave Enemy ID")]
    public void SpawnWavesById(int id, ScriptableWaves waves)
    {
        for (int j = 0; j < waves.waves[id].enemys.Count; j++)
        {
            GameObject enemy = PoolController.instance.GetEnemy(waves.waves[id].enemys[j].enemyPool);

            if(!waves.waves[id].enemys[j].randomPos)
                enemy.transform.position = spawnPoints[waves.waves[id].enemys[j].spawnPoint].position;
            else
                enemy.transform.position = spawnPoints[Random.Range(1, spawnPoints.Count)].position;
        }
    }

    [Button(Name = "Spawn Wave Enemy Difficult Level")]
    public void SpawnWavesByDificult(int level, ScriptableWaves waves)
    {
        
        //Create a list of waves with dificultLevel selected
        wavesByLevel.Clear();
        for (int i = 0; i < waves.waves.Count; i++)
        {
            if (waves.waves[i].difficultLevel == level)
            {
                wavesByLevel.Add(waves.waves[i]);
            }
        }

        if (wavesByLevel.Count == 0)
        {
            Debug.Log("You dont have waves in this difficult level!" + level);
            return;
        }

        int randNumb = Random.Range(0, wavesByLevel.Count);

        //SpawnRandom in waveByLevel
        for (int i = 0; i < wavesByLevel[randNumb].enemys.Count; i++)
        {
            GameObject enemy = PoolController.instance.GetEnemy(wavesByLevel[randNumb].enemys[i].enemyPool);

            if (!wavesByLevel[randNumb].enemys[i].randomPos)
                enemy.transform.position = spawnPoints[wavesByLevel[randNumb].enemys[i].spawnPoint].position;
            else
                enemy.transform.position = spawnPoints[Random.Range(1, spawnPoints.Count)].position;
        }
    }

    [Button(Name = "Spawn Wave Enemy Random")]
    public void SpawnWavesRandom(ScriptableWaves waves)
    {
        int randNumb = Random.Range(0, waves.waves.Count);

        for (int i = 0; i < waves.waves[randNumb].enemys.Count; i++)
        {
            GameObject enemy = PoolController.instance.GetEnemy(waves.waves[randNumb].enemys[i].enemyPool);

            if (!waves.waves[randNumb].enemys[i].randomPos)
                enemy.transform.position = spawnPoints[waves.waves[randNumb].enemys[i].spawnPoint].position;
            else
                enemy.transform.position = spawnPoints[Random.Range(1, spawnPoints.Count)].position;
        }
        
    }

}
