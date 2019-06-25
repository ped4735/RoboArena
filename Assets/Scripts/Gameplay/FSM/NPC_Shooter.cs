using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[System.Serializable]
public class Shoot
{
    [HideLabel]
    public string shootType;
    public PoolTypes bulletType;
    public List<Transform> aimPoints = new List<Transform>();
    public float fireSpeed = 0.5f;
    public int damage = 10;
    [ShowIf("bulletType", PoolTypes.Bullet_EnemyNormal)]
    public Color color = Color.red;
}


public class NPC_Shooter : MonoBehaviour
{
    [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "shootType")]
    public List<Shoot> shootTypes = new List<Shoot>();
    private int selectedType;

    public void StartShoot(int type)
    {
        selectedType = type;
        InvokeRepeating("Shoot", 0.5f, shootTypes[selectedType].fireSpeed);
    }

    public void StopShoot()
    {
        CancelInvoke("Shoot");
    }

    private void OnDisable()
    {
        CancelInvoke("Shoot");
    }

    private void Shoot()
    {

        if (shootTypes[selectedType].aimPoints.Count == 0)
            return;

        for (int i = 0; i < shootTypes[selectedType].aimPoints.Count; i++)
        {
            GameObject bullet = PoolController.instance.GetBullet(shootTypes[selectedType].bulletType);
            bullet.transform.position = shootTypes[selectedType].aimPoints[i].position;
            bullet.transform.rotation = shootTypes[selectedType].aimPoints[i].rotation;
            bullet.GetComponent<DamageOnTouch>().SetDamage(shootTypes[selectedType].damage);

            if (shootTypes[selectedType].bulletType == PoolTypes.Bullet_EnemyNormal)
            {
                bullet.GetComponentInChildren<ParticleSystem>().startColor = shootTypes[selectedType].color;
            }
        }

    }
        
}
