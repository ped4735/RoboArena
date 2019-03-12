using System.Collections.Generic;
using UnityEngine;

public class NPC_Shooter : MonoBehaviour
{

    private Pool enemyBulletPool;

    public List<Transform> aimPoints = new List<Transform>();
    public float fireSpeed;

    private void Awake()
    {
        enemyBulletPool = GameObject.FindGameObjectWithTag("EnemyBulletPool").GetComponent<Pool>();
    }

    public void StartShoot()
    {
        InvokeRepeating("Shoot", 0.5f, fireSpeed);
    }

    public void StopShoot()
    {
        CancelInvoke("Shoot");
    }

    private void Shoot()
    {
        if (aimPoints.Count == 0)
            return;

        for (int i = 0; i < aimPoints.Count; i++)
        {
            GameObject bullet = enemyBulletPool.nextThing;
            bullet.transform.position = aimPoints[i].position;
            bullet.transform.rotation = aimPoints[i].rotation;
        }


    }
        
}
