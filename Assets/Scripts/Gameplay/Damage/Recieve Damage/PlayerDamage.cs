using UnityEngine;

public class PlayerDamage : DamageManager
{
    public bool immortal;
    public PoolTypes deathVFX;
    public int maxHP = 100;

    private int currentHP;

    private void Start()
    {
        currentHP = maxHP;
    }

    public override void Hit(int damage)
    {
        currentHP -= damage;
        UIController.instance.SetLifePlayerValueUI(currentHP);

        if (!immortal)
        {
            if (currentHP <= 0)
            {
                Death();
            }
        }
    }

    public override void Death()
    {
        UIController.instance.GameOver();
    }
}
