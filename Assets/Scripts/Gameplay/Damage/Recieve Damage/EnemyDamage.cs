using UnityEngine;
using Sirenix.OdinInspector;

public class EnemyDamage : DamageManager
{
    public bool disableOnDeath;
    public bool disableParentInstead;

    public bool vfxOnDeath;
    [ShowIf("vfxOnDeath")]
    public PoolTypes vfxType;

    public bool vfxOnEnable;
    [ShowIf("vfxOnEnable")]
    public PoolTypes vfxTypeOnEnable;

    public int maxHP;
    public int gearPointsReward;
    private int currentHP;

    
    private void OnEnable()
    {
        currentHP = maxHP;
        WaveManager.instance.AddEnemyCount();

        if(vfxOnEnable)
            Invoke("VFXStart", 0.1f);        
    }

    private void OnDisable()
    {
        WaveManager.instance.RemoveEnemyCount();

        if (vfxOnDeath)
        {
            GameObject vfx = PoolController.instance.GetVFX(vfxType);
            vfx.transform.position = transform.position;
        }
    }

    public override void Hit(int damage)
    {
        currentHP -= damage;

        if(currentHP <= 0)
        {
            currentHP = 0;
            Death();
        }

        UIController.instance.SetLifeEnemyValueUI(currentHP);
    }

    public override void Death()
    {
        UpgradeController.instance.AddPoints(gearPointsReward);

        if (disableParentInstead)
        {
            if (disableOnDeath)
            {
                transform.parent.gameObject.SetActive(false);
            }
            else
            {
                Destroy(transform.parent.gameObject);
            }
        }
        else
        {
            if (disableOnDeath)
            {
                gameObject.SetActive(false);
            }
            else
            {
                Destroy(gameObject);
            }
        } 
    }

    public void VFXStart()
    {
        GameObject vfx = PoolController.instance.GetVFX(vfxTypeOnEnable);
        vfx.transform.position = transform.position;
    }

}
