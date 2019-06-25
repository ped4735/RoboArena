using System.Collections;
using UnityEngine;
using TMPro;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using DG.Tweening;

public class UIController : MonoBehaviour
{
    
    public static UIController instance;

    public TextMeshProUGUI life_player_UI;
    public TextMeshProUGUI life_enemy_UI;
    public TextMeshProUGUI gear_UI;
    public GameObject panelEnemyLife;
    public GameObject panelPlayerLife;
    public GameObject panelGear;
    public GameObject panelUpgrade;
    public GameObject alertIcon;
    public TextMeshProUGUI waveUI, waveTimeUI;
    public TextMeshProUGUI waveYouDiedUI;
    public GameObject gameOverUI;
    public GameObject pauseMenu;

    //audio
    public Sprite muteON, muteOFF;
    public Image soundIcon;

    [HideInInspector]
    public bool paused;

    private void Start()
    {
        AudioManager.instance.PlayByID(1);
        alertIcon.SetActive(false);
        DataManager.instance.LoadGame();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //Debug.Break();
            PauseGame(); 
        }
    }

    public void ShowUpgradeMenu()
    {
        paused = !paused;
        panelUpgrade.SetActive(paused);
        panelPlayerLife.SetActive(!paused);
        panelEnemyLife.SetActive(false);
        waveUI.GetComponent<CanvasGroup>().alpha = paused ? 0 : 1;

        if (paused)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }


    }

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    public void SetLifePlayerValueUI(int value)
    {
        life_player_UI.text = value.ToString("000");
    }

    public void SetLifeEnemyValueUI(int value)
    {
        if (!panelEnemyLife.activeSelf)
        {
            panelEnemyLife.SetActive(true);
            StartCoroutine("timeToDisable", new object[2] { panelEnemyLife, 2f});
        }

        life_enemy_UI.text = value.ToString("000");
    }

    IEnumerator timeToDisable(object[] paramns)
    {
        yield return new WaitForSeconds((float)paramns[1]);
        GameObject obj = (GameObject)paramns[0];
        obj.SetActive(false);
    }

    [Button(Name = "Pause Game")]
    public void PauseGame()
    {
        paused = !paused;
        pauseMenu.SetActive(paused);
        EnableGamePlayUI(!paused);

        if (paused)
        {
            Time.timeScale = 0;
            if (AudioManager.instance.mute)
            {
                soundIcon.sprite = muteON;
            }
            else
            {
                soundIcon.sprite = muteOFF;
            }
        }
        else
        {
            Time.timeScale = 1;
        }

    }

    [Button(Name = "Game Over")]
    public void GameOver()
    {
        waveYouDiedUI.text = "YOU DIED IN WAVE #" + WaveManager.instance.currentWave;

        EnableGamePlayUI(false);
        gameOverUI.SetActive(true);
        Time.timeScale = 0;
    }

    public void LoadSceneName(string sceneName)
    {
        Time.timeScale = 1;
        AudioManager.instance.PauseMusic();
        DataManager.instance.SaveGame(false);
        Manager.instance.LoadScene(sceneName);
    }

    public void GameOverLoadSceneName(string sceneName)
    {
        Time.timeScale = 1;
        AudioManager.instance.PauseMusic();
        DataManager.instance.SaveGame(true);
        Manager.instance.LoadScene(sceneName);
    }

    public void MuteSound()
    {
        AudioManager.instance.Mute();

        if (AudioManager.instance.mute)
        {
            soundIcon.sprite = muteON;
        }
        else
        {
            soundIcon.sprite = muteOFF;
        }
    }

    public void UpdateWaveUI(int level, int wave)
    {
        waveUI.gameObject.SetActive(true);
        waveUI.text = "WAVE - " + wave;
    }

    public void UpdateTimeWaveUI(float time)
    {
        waveTimeUI.text = time + "";
    }

    public void EneableWaveUI(bool enable)
    {
        waveUI.gameObject.SetActive(enable);
    }

    public void EnableGamePlayUI(bool enable)
    {
        panelPlayerLife.SetActive(enable);
        panelGear.SetActive(enable);
        panelEnemyLife.SetActive(false);
        waveUI.GetComponent<CanvasGroup>().alpha = enable ? 1 : 0;
    }

    public void SetPointsValue(int value)
    {
        gear_UI.GetComponent<DOTweenAnimation>().DOPlay();
        gear_UI.text = value.ToString("00000");
    }

    public void ShowAlertIcon(bool show)
    {
        alertIcon.SetActive(show);
    }
}
