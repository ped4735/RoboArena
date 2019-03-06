using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
    public float timeToDye;

    private void Start()
    {
        Destroy(gameObject, timeToDye);
    }
}
