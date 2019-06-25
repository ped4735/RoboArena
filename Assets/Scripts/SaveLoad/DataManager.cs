using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public List<Upgrade> upgrades;
    public int waveNumber;
    public int waveLevel;
    public int waveLevelUp;
    public int playerHP;
    public int GP;
}

public class DataManager : MonoBehaviour
{
    public static DataManager instance;
    private string folder = "data";
    private string archiveName = "save.dat";
    [HideInInspector]
    public GameData dataToSave = new GameData();
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
    }

    public void SaveGame(bool reset)
    {        
        string folderPath = Path.Combine(Application.persistentDataPath, folder);
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string dataPath = Path.Combine(folderPath, archiveName);

        if (reset)
        {
            ResetSaveData();
        }
        else
        {
            GetSaveData();
        }
            
        //Binary Save ############################################################
        BinaryFormatter binaryFormatter = new BinaryFormatter();

        using (FileStream fileStream = File.Open(dataPath, FileMode.OpenOrCreate))
        {
            binaryFormatter.Serialize(fileStream, dataToSave);
            //Debug.Log("Save data in: " + dataPath);
        }
        //########################################################################
    }

    public void LoadGame()
    {
        string folderPath = Path.Combine(Application.persistentDataPath, folder);
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string dataPath = Path.Combine(folderPath, archiveName);

        //Load Binary ############################################################
        BinaryFormatter binaryFormatter = new BinaryFormatter();

        try
        {
            using (FileStream fileStream = File.Open(dataPath, FileMode.Open))
            {
                dataToSave = (GameData)binaryFormatter.Deserialize(fileStream);
            }
            //########################################################################
            SetLoadData();
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }
    }

    private void GetSaveData()
    {
        GameObject player = UpgradeController.instance.player;
        dataToSave.playerHP = player.GetComponent<PlayerDamage>().CurrentHP;
        dataToSave.GP = UpgradeController.instance.gearPoints;
        dataToSave.upgrades = new List<Upgrade>(UpgradeController.instance.upgrades);
        dataToSave.waveNumber = WaveManager.instance.currentWave;
        dataToSave.waveLevel = WaveManager.instance.currentLevel;
        dataToSave.waveLevelUp = WaveManager.instance.manyWavesToDifficultLevelUp;
    }

    private void SetLoadData()
    {
        GameObject player = UpgradeController.instance.player;
        //Debug.Log(dataToSave.playerHP);
        //UpgradeController.instance.upgrades = new List<Upgrade>(dataToSave.upgrades);
        WaveManager.instance.currentWave = dataToSave.waveNumber;
        WaveManager.instance.currentLevel = dataToSave.waveLevel;
        WaveManager.instance.manyWavesToDifficultLevelUp = dataToSave.waveLevelUp;

        //Carregar Upgrades
        for (int i = 0; i < dataToSave.upgrades.Count; i++)
        {
            for (int j = 0; j < dataToSave.upgrades[i].currentLevel; j++)
            {
                switch (i)
                {
                    case 0:
                        UpgradeController.instance.UpgradeHP();
                        break;
                    case 1:
                        UpgradeController.instance.UpgradeRegen();
                        break;
                    case 2:
                        UpgradeController.instance.UpgradeDamage();
                        break;
                    case 3:
                        UpgradeController.instance.UpgradeRange();
                        break;
                    case 4:
                        UpgradeController.instance.UpgradeSpeedATK();
                        break;

                    default:
                        break;
                }
            }
        }
        player.GetComponent<PlayerDamage>().CurrentHP = dataToSave.playerHP;
        UpgradeController.instance.gearPoints = dataToSave.GP;


        //Atualizar UI
        UIController.instance.SetLifePlayerValueUI(dataToSave.playerHP);
        UIController.instance.SetPointsValue(dataToSave.GP);
    }

    private void ResetSaveData()
    {
        dataToSave.playerHP = 100;
        dataToSave.GP = 0;
        dataToSave.waveNumber = 0;
        dataToSave.waveLevel = 0;
        dataToSave.waveLevelUp = 2;

        for (int i = 0; i < dataToSave.upgrades.Count; i++)
        {
            dataToSave.upgrades[i].currentLevel = 0;
        }

    }
}
