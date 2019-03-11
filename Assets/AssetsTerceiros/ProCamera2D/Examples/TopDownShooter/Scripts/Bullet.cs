using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;

public class Bullet : MonoBehaviour
{
    public float bulletDuration = 1f;
    public float bulletSpeed = 50f;

    public LayerMask collisionMask;
    //public string tagToCollider;
    public string[] tagToColliders;

    public int BulletDamage = 10;

    private bool _exploding;


    void OnEnable()
    {
        _exploding = false;
    }

    void Update()
    {
        if (_exploding)
            return;

        transform.position += transform.forward * Time.deltaTime * bulletSpeed;

    }

    private void OnTriggerEnter(Collider col)
    {
        for (int i = 0; i < tagToColliders.Length; i++)
        {
            if (col.CompareTag(tagToColliders[i]))
            {
                DamageManager damageManagerRef = col.GetComponent<DamageManager>();
                if(damageManagerRef != null)
                {
                    damageManagerRef.Hit(this.gameObject);
                }
                Disable();
            }
        }
        
        //Debug.Log("Colidiu com:" + col.transform.name);
    }

    //void Collide()
    //{
    //    _exploding = true;
    //    _transform.position = _collisionPoint;

    //    _raycastHit.collider.SendMessageUpwards("Hit", BulletDamage, SendMessageOptions.DontRequireReceiver);

    //    Disable();
    //}

    public void Disable()
    {
        _exploding = true;
        gameObject.SetActive(false);
    }
}
