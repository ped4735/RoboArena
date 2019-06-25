using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public struct EnemysInLevel
{
  
    public bool random;
    
    [Tooltip("Os inimigos marcados com essa propriedade não podem ser spawnados no mesmo lugar.")]
    public bool staticEnemy;

    public PoolTypes enemy;


    [HideIf("random")]
    public bool randomQuantity;

    [HideIf("random")]
    [HideIf("randomQuantity")]
    [HideLabel]
    public int quantity;    

    [ShowIf("randomQuantity")]
    [HideIf("random")]
    [HideLabel]
    public int initial;

    [ShowIf("randomQuantity")]
    [HideIf("random")]
    [HideLabel]
    public int final;

}

[System.Serializable]
public struct WaveLevel
{
    [Title("")]
    [Tooltip("Nivel de dificuldade da Wave.")]
    public int waveLevel;
    [Tooltip("Quantidade de inimigos que serão spawnados de forma aleatória. <marcado> Quantidade de inimigos aleatórios serão gerados randomicamente entre dois numeros.")]
    public bool enemyQuantityRandom;
    [HideIf("enemyQuantityRandom")]
    [HideLabel]
    public int quantityRandom;

    [ShowIf("enemyQuantityRandom")]
    [HideLabel]
    public int quantityRandomInitial;

    [ShowIf("enemyQuantityRandom")]
    [HideLabel]
    public int quantityRandomFinal;

    public List<EnemysInLevel> enemysInLevel;
}

[CreateAssetMenu(menuName = "Wave/LevelWave", fileName = "LevelWave")]
public class ScriptableGenerateWaves : ScriptableObject
{
    public List<WaveLevel> wavesLevel = new List<WaveLevel>();
}
