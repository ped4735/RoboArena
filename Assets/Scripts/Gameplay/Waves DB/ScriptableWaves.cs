using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;



[System.Serializable]
public struct Spawn
{

    public bool randomPos;
    [HideIf("randomPos")]
    public int spawnPoint;
    public PoolTypes enemyPool;

}

[System.Serializable]
public struct Waves
{
    public int difficultLevel;
    public List<Spawn> enemys;
}

[CreateAssetMenu(menuName = "Wave/DataBase", fileName = "WaveDB")]
public class ScriptableWaves : ScriptableObject
{
    public List<Waves> waves = new List<Waves>();
}
