using System.Collections;
using UnityEngine;
using TMPro;
using Sirenix.OdinInspector;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    
    public static UIController instance;

    public TextMeshProUGUI life_player_UI;
    public TextMeshProUGUI life_enemy_UI;
    public TextMeshProUGUI waveUI, waveTimeUI;
    public GameObject gameOverUI;
    public GameObject pauseMenu;
    

    [HideInInspector]
    public bool paused;

    private void Start()
    {
        AudioManager.instance.PlayByID(1);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseGame(); 
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
        life_enemy_UI.text = value.ToString("000");
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
        }
        else
        {
            Time.timeScale = 1;
        }

    }

    [Button(Name = "Game Over")]
    public void GameOver()
    {
        EnableGamePlayUI(false);
        gameOverUI.SetActive(true);
        Time.timeScale = 0;
    }

    public void LoadSceneName(string sceneName)
    {
        Time.timeScale = 1;
        AudioManager.instance.PauseMusic();
        Manager.instance.LoadScene(sceneName);
    }

    public void MuteSound()
    {
        AudioManager.instance.Mute();
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
        life_enemy_UI.gameObject.SetActive(enable);
        life_player_UI.gameObject.SetActive(enable);
        waveUI.GetComponent<CanvasGroup>().alpha = enable ? 1 : 0;
    }
}
