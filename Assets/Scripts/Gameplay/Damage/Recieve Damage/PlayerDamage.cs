using UnityEngine;
using System;
using System.Collections;

public class PlayerDamage : DamageManager
{
    public bool immortal;
    public PoolTypes deathVFX;
    public int maxHP = 100;
    public Color damageColor;
    public SkinnedMeshRenderer mesh;

    [HideInInspector]
    private int currentHP = 100;

    public int CurrentHP {
        get => currentHP;
        set => currentHP = value >= maxHP ? maxHP:value;
    }


    public override void Hit(int damage)
    {
        CurrentHP -= damage;
        UIController.instance.SetLifePlayerValueUI(CurrentHP);

        if (!immortal)
        {
            if (CurrentHP <= 0)
            {
                Death();
            }
        }
    }

    public override void Death()
    {
        CurrentHP = maxHP;
        UIController.instance.GameOver();
    }

    public void SetRegenRate(float timeRegen)
    {
        StopCoroutine("RegenCoroutine");
        StartCoroutine("RegenCoroutine", timeRegen);
    }

    public void SetHP(int hp)
    {
        maxHP = hp;
        currentHP = maxHP;
        UIController.instance.SetLifePlayerValueUI(maxHP);
    }

    private  IEnumerator RegenCoroutine(float time)
    {
        while (true)
        {
            CurrentHP += 1;
            UIController.instance.SetLifePlayerValueUI(CurrentHP);
            yield return new WaitForSeconds(time);
        }       
    }

}
