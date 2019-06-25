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

    //Wave Level Generator
    private List<WaveLevel> wavesLevel = new List<WaveLevel>();
    private List<EnemysInLevel> enemysRandom = new List<EnemysInLevel>();
    private List<Transform> staticEnemyPositions = new List<Transform>();
    

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
    
    //[Button(Name = "Spawn Wave Enemy ID")]
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

    //[Button(Name = "Spawn Wave Enemy Difficult Level")]
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

    //[Button(Name = "Spawn Wave Enemy Random")]
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

    [Button(Name = "Spawn Wave Enemy Difficult Level")]
    public void GenerateWaveByLevelRandom(int level, ScriptableGenerateWaves waves)
    {
        
        wavesLevel.Clear();
        for (int i = 0; i < waves.wavesLevel.Count; i++)
        {
            if (waves.wavesLevel[i].waveLevel == level)
            {
                wavesLevel.Add(waves.wavesLevel[i]);
            }
        }
        if (wavesLevel.Count == 0)
        {
            Debug.Log("You dont have waves in this difficult level!" + level);
            return;
        }

        int randNumb = Random.Range(0, wavesLevel.Count);


        //Spawn Required Enemys
        staticEnemyPositions = new List<Transform>(spawnPoints);
        for (int j = 0; j < wavesLevel[randNumb].enemysInLevel.Count; j++)
        {
            if (!wavesLevel[randNumb].enemysInLevel[j].random)
            {
                int quantity;

                if (wavesLevel[randNumb].enemysInLevel[j].randomQuantity)
                {
                    
                    quantity = Random.Range(wavesLevel[randNumb].enemysInLevel[j].initial, wavesLevel[randNumb].enemysInLevel[j].final + 1);
                }
                else
                {
                    quantity = wavesLevel[randNumb].enemysInLevel[j].quantity;
                }

                for (int l = 0; l < quantity; l++)
                {

                    GameObject enemy = PoolController.instance.GetEnemy(wavesLevel[randNumb].enemysInLevel[j].enemy);


                    if (wavesLevel[randNumb].enemysInLevel[j].staticEnemy)
                    {
                        if(staticEnemyPositions.Count > 0)
                        {
                            int randPos = Random.Range(1, staticEnemyPositions.Count);
                            enemy.transform.position = staticEnemyPositions[randPos].position;
                            staticEnemyPositions.RemoveAt(randPos);
                        }                           
                    }
                    else
                    {
                        enemy.transform.position = spawnPoints[Random.Range(1, spawnPoints.Count)].position;
                    }
                }


            }
        }



        //Spawn Random Enemys
        enemysRandom.Clear();
        for (int i = 0; i < wavesLevel[randNumb].enemysInLevel.Count; i++)
        {
            if (wavesLevel[randNumb].enemysInLevel[i].random)
            {
                enemysRandom.Add(wavesLevel[randNumb].enemysInLevel[i]);
            }
        }

        
        if (enemysRandom.Count != 0)
        {
            int enemyQuantity;
            
            if (wavesLevel[randNumb].enemyQuantityRandom)
            {
                enemyQuantity = Random.Range(wavesLevel[randNumb].quantityRandomInitial, wavesLevel[randNumb].quantityRandomFinal + 1);
            }
            else
            {
                enemyQuantity = wavesLevel[randNumb].quantityRandom;
            }

            for (int i = 0; i < enemyQuantity; i++)
            {
                int randSelect = Random.Range(0, enemysRandom.Count);

                GameObject enemy = PoolController.instance.GetEnemy(enemysRandom[randSelect].enemy);

                if (enemysRandom[randSelect].staticEnemy)
                {
                    int randPos = Random.Range(1, staticEnemyPositions.Count);
                    enemy.transform.position = staticEnemyPositions[randPos].position;
                    staticEnemyPositions.RemoveAt(randPos);
                }
                else
                {
                    enemy.transform.position = spawnPoints[Random.Range(1, spawnPoints.Count)].position;
                }
                
            } 
        }
            
    }
}
