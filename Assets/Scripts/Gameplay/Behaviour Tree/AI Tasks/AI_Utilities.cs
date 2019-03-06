using UnityEngine;
using Panda;

public class AI_Utilities : MonoBehaviour
{
    [Task]
    public void Log(string msg)
    {
        Debug.Log(msg);
        Task.current.Succeed();
    }
}
