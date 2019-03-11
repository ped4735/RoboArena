using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDamage : DamageManager
{

    private int maxHP = 100;
    private int currentHP;

    private void Start()
    {
        currentHP = maxHP;
    }


    public override void Hit()
    {
        //Debug.Log("Player take a HIT");
    }

    public override void Hit(int damage)
    {
        //Debug.Log("Player take: " + damage + " on hit");
    }

    public override void Hit(GameObject bullet)
    {
        Bullet bulletRef = bullet.GetComponent<Bullet>();
        currentHP -= bulletRef.BulletDamage;
        GameplayController.instance.SetLifeValueUI(currentHP);
    }
}
