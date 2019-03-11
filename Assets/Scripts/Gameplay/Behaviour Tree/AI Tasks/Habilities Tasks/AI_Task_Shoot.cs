using UnityEngine;
using Panda;

public class AI_Task_Shoot : MonoBehaviour
{

    public Transform aimPosition;
    public Pool bulletPool;

    public float fireRate;

    private PandaBehaviour pBT;


    private void Start()
    {
        pBT = GetComponent<PandaBehaviour>();
    }
    
    [Task]
    private bool isShooting; 
    

    [Task]
    public void SetShoot(bool shoot)
    {
        isShooting = shoot;
        Task.current.Succeed();
    }

    [Task]
    public void Shoot()
    {
        GameObject bullet = bulletPool.nextThing;
        bullet.transform.position = aimPosition.position;
        bullet.transform.rotation = aimPosition.transform.rotation;

        Task.current.Succeed();
    }

    [Task]
    public void FireRateWaiting()
    {
        pBT.Wait(fireRate);
    }

}
