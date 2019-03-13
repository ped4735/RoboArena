using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Collider))]
public class DamageOnTouch : MonoBehaviour
{
    public bool disableAfterTouch;
    public string[] tagToColliders;
    public int damage;


    private void OnTriggerEnter(Collider col)
    {
        for (int i = 0; i < tagToColliders.Length; i++)
        {
            if (col.CompareTag(tagToColliders[i]))
            {
                DamageManager damageManagerRef = col.GetComponent<DamageManager>();
                if (damageManagerRef != null)
                {
                    damageManagerRef.Hit(damage);
                }

                if(disableAfterTouch)
                    Disable();
            }
        }
    }

    public void Disable()
    {
        gameObject.SetActive(false);
    }
}
