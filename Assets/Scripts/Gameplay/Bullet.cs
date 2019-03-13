using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;

public class Bullet : MonoBehaviour
{
    public float bulletDuration = 1f;
    public float bulletSpeed = 50f;

    //public string[] tagToColliders;

    //public int BulletDamage = 10;

    //private bool _exploding;
    private float time;

    void OnEnable()
    {
        //_exploding = false;
        time = 0;
    }

    //private void OnDisable()
    //{
        
    //}

    void Update()
    {

        //if (_exploding)
        //    return;

        time += Time.deltaTime;

        if(time >= bulletDuration)
        {
            Disable();
        }

        transform.position += transform.forward * Time.deltaTime * bulletSpeed;

    }

    //private void OnTriggerEnter(Collider col)
    //{
    //    for (int i = 0; i < tagToColliders.Length; i++)
    //    {
    //        if (col.CompareTag(tagToColliders[i]))
    //        {
    //            DamageManager damageManagerRef = col.GetComponent<DamageManager>();
    //            if(damageManagerRef != null)
    //            {
    //                damageManagerRef.Hit(BulletDamage);
    //            }
    //            Disable();
    //        }
    //    }

    //}

    public void Disable()
    {
        //_exploding = true;
        gameObject.SetActive(false);
    }
}
