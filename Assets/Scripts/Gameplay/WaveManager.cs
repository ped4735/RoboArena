using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class WaveManager : MonoBehaviour
{
    
    public static WaveManager instance;

    [Title("Control Parameters")]
    public bool startOnAwake;
    public float timeBtwWaves = 5f;
    public int  maxDificultLevel = 0;
    public int manyWavesToDifficultLevelUp = 3;


    [Title("Debug Parameters")]
    public int currentLevel = 0;
    public int currentWave = 0;
    public int enemyNumberInWave;


    [Title("Waves")]
    public ScriptableWaves normalWaves;
    public ScriptableWaves tutorialWaves;
    public ScriptableWaves bossWaves;

    private bool waveManagerDisable;


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

    }

    private void Start()
    {
        
        if (startOnAwake)
        {
            StartCoroutine(WaitForFirstWave());
        }
            
    }

    public void AddEnemyCount()
    {
        enemyNumberInWave++;
    }

    public void RemoveEnemyCount()
    {
        enemyNumberInWave--;

        if (enemyNumberInWave <= 0)
        {
            if(!waveManagerDisable)
                StartCoroutine("WaitForNextWave");
        }
    }

    IEnumerator WaitForFirstWave()
    {

        UIController.instance.UpdateWaveUI(currentLevel, currentWave);
        Coroutine C = StartCoroutine(WaitForTimeWave());
        yield return C;
        //Debug.Log("Level: " + currentLevel + " Wave: " + currentWave);
        yield return new WaitForSeconds(0.3f);
        UIController.instance.EneableWaveUI(false);
        WaveController.instance.SpawnWavesByDificult(currentLevel, normalWaves);
    }

    IEnumerator WaitForNextWave()
    {

        //Debug.Log("Acabou Wave" + currentWave);
        LevelCount();
        UIController.instance.UpdateWaveUI(currentLevel, currentWave);
        Coroutine C = StartCoroutine(WaitForTimeWave());
        yield return C;
        //Debug.Log("Level: " + currentLevel + " Wave: " + currentWave);
        yield return new WaitForSeconds(0.3f);
        UIController.instance.EneableWaveUI(false);
        WaveController.instance.SpawnWavesByDificult(currentLevel, normalWaves);
    }

    IEnumerator WaitForTimeWave()
    {
        float time = timeBtwWaves;
        UIController.instance.UpdateTimeWaveUI(time);

        while (time > 0)
        {
            yield return new WaitForSeconds(1f);
            time -= 1f;
            UIController.instance.UpdateTimeWaveUI(time);
        }
    }

    public void LevelCount()
    {
        currentWave++;

        if(currentWave >= manyWavesToDifficultLevelUp)
        {
            currentLevel++;

            if(currentLevel > maxDificultLevel)
            {
                currentLevel = maxDificultLevel;
            }
        }
    }

    private void OnDisable()
    {
        waveManagerDisable = true;
        StopAllCoroutines();
    }
}
