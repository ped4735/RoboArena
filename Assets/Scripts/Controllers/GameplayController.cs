using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Sirenix.OdinInspector;
using UnityEngine.UI;

public class GameplayController : MonoBehaviour
{

    public static GameplayController instance;

    public TextMeshProUGUI life_player_UI;
    public GameObject player;
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

    public void SetLifeValueUI(int value)
    {
        life_player_UI.text = value.ToString("000");
    }

    [Button(Name = "Pause Game")]
    public void PauseGame()
    {
        paused = !paused;
        pauseMenu.SetActive(paused);

        if (paused)
        {
            Time.timeScale = 0;

        }
        else
        {
            Time.timeScale = 1;
        }

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

}
