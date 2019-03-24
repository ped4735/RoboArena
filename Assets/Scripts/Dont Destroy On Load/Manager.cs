using UnityEngine;
using UnityEngine.SceneManagement;

public class Manager : MonoBehaviour
{
    public static Manager instance;

    private int tutorialEnable;

    void Awake()
    {
        #region Singleton
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        #endregion
        //LoadPlayerConfig();
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public bool GetTutorialState()
    {
        return tutorialEnable == 1?true:false;
    }

    public void SavePlayerConfig()
    {
        PlayerPrefs.SetInt("tutorial", tutorialEnable);
        PlayerPrefs.Save();
    }

    public void LoadPlayerConfig()
    {
        tutorialEnable = PlayerPrefs.GetInt("tutorial");
    }

}
