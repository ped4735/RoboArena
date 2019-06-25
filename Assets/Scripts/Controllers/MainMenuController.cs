using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{
   public void StartGame()
   {
        Manager.instance.LoadScene("Gameplay");
   }
    
   public void LoadSceneDelayed(string sceneName)
   {
        StartCoroutine(loadSceneAfterTime(sceneName));
   }

    IEnumerator loadSceneAfterTime(string sceneName)
    {
        yield return new WaitForSeconds(2f);
        Manager.instance.LoadScene(sceneName);
    }
}
