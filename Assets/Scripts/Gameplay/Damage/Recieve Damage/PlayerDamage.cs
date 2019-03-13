using UnityEngine;

public class PlayerDamage : DamageManager
{
    public GameObject deathVFX;
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

        if (currentHP == 0)
        {
            //TODO Death
        }
    }
}
