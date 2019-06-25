using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(ParticleSystem))]
public class ParticleColisionDamage : MonoBehaviour
{

    private ParticleSystem pSystem;
    private List<ParticleCollisionEvent> colEvents = new List<ParticleCollisionEvent>();

    public int damage;
    public string[] tagToCollider;

    private void Awake()
    {
        pSystem = GetComponent<ParticleSystem>();
    }

    private void OnParticleCollision(GameObject col)
    {
        int colSize = pSystem.GetSafeCollisionEventSize();

        if(colSize > colEvents.Count)
        {
            colEvents = new List<ParticleCollisionEvent>(colSize);
        }

        int eventCount = pSystem.GetCollisionEvents(col, colEvents);

        for (int i = 0; i < eventCount; i++)
        {

            for (int j = 0; j < tagToCollider.Length; j++)
            {
                if (col.CompareTag(tagToCollider[j]))
                {
                    DamageManager dmg = col.GetComponent<DamageManager>();

                    if (dmg)
                    {
                        col.GetComponent<DamageManager>().Hit(damage);
                    }
                }
            }
            
        }
    }
}
