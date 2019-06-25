using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;

public class WaveManager : MonoBehaviour
{
    
    public static WaveManager instance;

    [Title("Control Parameters")]
    public bool startOnAwake;
    public float timeBtwWaves = 5f;
    public int manyWavesToDifficultLevelUp = 3;
    public int stepWavesToDifficultLevelUp = 0;


    [Title("Debug Parameters")]
    [ReadOnly]
    public int currentLevel = 0;
    [ReadOnly]
    public int currentWave = 0;
    [ReadOnly]
    public int enemyNumberInWave;
    private int countWaves = 0;
    [ReadOnly]
    public int maxDificultLevel = 0;

    
    private ScriptableWaves normalWaves;
    private ScriptableWaves tutorialWaves;
    private ScriptableWaves bossWaves;
    [Title("Waves")]
    public ScriptableGenerateWaves waveLevel;

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

        int countDiffNumber = 0;

        for (int i = 0; i < waveLevel.wavesLevel.Count; i++)
        {
            countDiffNumber = Mathf.Max(countDiffNumber, waveLevel.wavesLevel[i].waveLevel);     
        }

        maxDificultLevel = countDiffNumber;

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
        yield return new WaitForSeconds(0.3f);
        UIController.instance.EneableWaveUI(false);
        WaveController.instance.GenerateWaveByLevelRandom(currentLevel, waveLevel);
    }

    IEnumerator WaitForNextWave()
    {
        
        LevelCount();
        UIController.instance.UpdateWaveUI(currentLevel, currentWave);
        Coroutine C = StartCoroutine(WaitForTimeWave());
        yield return C;
        yield return new WaitForSeconds(0.3f);
        UIController.instance.EneableWaveUI(false);
        WaveController.instance.GenerateWaveByLevelRandom(currentLevel, waveLevel);
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
        countWaves++;

        if (countWaves >= manyWavesToDifficultLevelUp)
        {
            currentLevel++;
            countWaves = 0;
            manyWavesToDifficultLevelUp += stepWavesToDifficultLevelUp;

            if (currentLevel > maxDificultLevel)
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
