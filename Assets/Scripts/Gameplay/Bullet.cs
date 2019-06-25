using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;

public class Bullet : MonoBehaviour
{
    public float bulletDuration = 1f;
    public float bulletSpeed = 50f;

    private float time;

    void OnEnable()
    {
        time = 0;
    }

    void Update()
    {

        time += Time.deltaTime;

        if(time >= bulletDuration)
        {
            Disable();
        }

        transform.position += transform.forward * Time.deltaTime * bulletSpeed;

    }

    public void SetDurationOfBullet(float time)
    {
        bulletDuration = time;
    }

    public void Disable()
    {
        gameObject.SetActive(false);
    }
}
