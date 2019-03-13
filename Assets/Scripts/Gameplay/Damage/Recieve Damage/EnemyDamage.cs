using UnityEngine;
using Sirenix.OdinInspector;

public class EnemyDamage : DamageManager
{
    public bool disableOnDeath;
    public bool disableParentInstead;

    public bool vfxOnDeath;
    [ShowIf("vfxOnDeath")]
    public Pool particleOnDeath;

    public int maxHP;
    private int currentHP;

    private void Awake()
    {
        currentHP = maxHP;
    }

    private void OnEnable()
    {
        currentHP = maxHP;
    }

    public override void Hit(int damage)
    {
        currentHP -= damage;

        if(currentHP <= 0)
        {
            Death();
        }

        UIController.instance.SetLifeEnemyValueUI(currentHP);
    }

    public void Death()
    {

        if (vfxOnDeath)
        {
            GameObject vfx = particleOnDeath.nextThing;
            vfx.transform.position = transform.position;
        }

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

}
