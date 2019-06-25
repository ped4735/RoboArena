using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class Upgrade
{
    [HideLabel]
    public string upgrade;
    public int[] prices;
    public bool useIntValues;
    [ShowIf("useIntValues")]
    public int[] intValues;
    public bool useFloatValues;
    [ShowIf("useFloatValues")]
    public float[] floatValues;    
    [ReadOnly]
    public int currentLevel;
}

[System.Serializable]
public class UpgradeTexts {
    [HideLabel]
    public string upgrade;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI priceText;
}

public class UpgradeController : MonoBehaviour
{
    public static UpgradeController instance;

    [HideInInspector]
    public int gearPoints = 0;
    public GameObject player;

    [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName ="upgrade")]
    public List<Upgrade> upgrades = new List<Upgrade>();
    [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "upgrade")]
    public List<UpgradeTexts> upgradeText = new List<UpgradeTexts>();

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
        ValidateUpgrades();       
    }

    [Button(Name = "Add Points")]
    public void AddPoints(int amount)
    {
        gearPoints += amount;
        UIController.instance.SetPointsValue(gearPoints);
        ValidateUpgrades();
    }

    [Button(Name = "Remove Points")]
    public void RemovePoints(int amount)
    {
        gearPoints -= amount;
        UIController.instance.SetPointsValue(gearPoints);
        ValidateUpgrades();
    }


    #region Upgrade Buttons

    public void UpgradeHP()
    {
        int id = 0;
        player.GetComponent<PlayerDamage>().SetHP(upgrades[id].intValues[upgrades[id].currentLevel]);
        UpdateUpgradeValues(upgrades[id]);
    }

    public void UpgradeRegen()
    {
        int id = 1;
        player.GetComponent<PlayerDamage>().SetRegenRate(upgrades[id].floatValues[upgrades[id].currentLevel]);
        UpdateUpgradeValues(upgrades[id]);
    }

    public void UpgradeDamage()
    {
        int id = 2;
        player.GetComponent<PlayerJoyControl>().SetPlayerDamage(upgrades[id].intValues[upgrades[id].currentLevel]);
        UpdateUpgradeValues(upgrades[id]);
    }

    public void UpgradeRange()
    {
        int id = 3;
        player.GetComponent<PlayerJoyControl>().SetPlayerRange(upgrades[id].floatValues[upgrades[id].currentLevel]);
        UpdateUpgradeValues(upgrades[id]);
    }

    public void UpgradeSpeedATK()
    {
        int id = 4;
        player.GetComponent<PlayerJoyControl>().SetPlayerAtkSpeedByAnim(upgrades[id].floatValues[upgrades[id].currentLevel]);
        UpdateUpgradeValues(upgrades[id]);
    }

    #endregion


    private void UpdateUpgradeUI(UpgradeTexts upgradeTexts, Upgrade upgrade)
    {
        if (upgrade.currentLevel >= upgrade.prices.Length)
        {
            upgradeTexts.levelText.text = "Max";
            upgradeTexts.priceText.text = "";
        }
        else
        {
            upgradeTexts.levelText.text = upgrade.currentLevel.ToString();
            upgradeTexts.priceText.text = upgrade.prices[upgrade.currentLevel].ToString();
        }
        
        
    }
    private void UpdateUpgradeValues(Upgrade upgrade)
    {
        RemovePoints(upgrade.prices[upgrade.currentLevel]);
        upgrade.currentLevel++;
        ValidateUpgrades();
    }
    private void ValidateUpgrades()
    {

        bool showAlert = false;

        for (int i = 0; i < upgrades.Count; i++)
        {
            

            UpdateUpgradeUI(upgradeText[i], upgrades[i]);

            if(upgrades[i].currentLevel >= upgrades[i].prices.Length)
            {
                upgradeText[i].levelText.transform.parent.GetComponent<Button>().interactable = false;
            }
            else
            {
                if (upgrades[i].prices[upgrades[i].currentLevel] > gearPoints)
                {
                    upgradeText[i].levelText.transform.parent.GetComponent<Button>().interactable = false;
                }
                else
                {
                    upgradeText[i].levelText.transform.parent.GetComponent<Button>().interactable = true;
                    showAlert = true;
                }
            }            
        }

        UIController.instance.ShowAlertIcon(showAlert);
    }

    private void OnApplicationQuit()
    {
        DataManager.instance.SaveGame(false);
    }

}
