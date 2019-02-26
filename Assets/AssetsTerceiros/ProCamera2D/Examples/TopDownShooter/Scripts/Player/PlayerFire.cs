using UnityEngine;
using System.Collections;

namespace Com.LuisPedroFonseca.ProCamera2D.TopDownShooter
{
    public class PlayerFire : MonoBehaviour
    {
        public Pool BulletPool;
        public Transform WeaponTip;

        public float FireRate = .3f;


        Transform _transform;

        void Awake()
        {
            _transform = transform;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                StartCoroutine(Fire());
            }
        }

        IEnumerator Fire()
        {
            while (Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0))
            {
                var bullet = BulletPool.nextThing; 
                bullet.transform.position = WeaponTip.position;
                bullet.transform.rotation = _transform.rotation;

                yield return new WaitForSeconds(FireRate);
            }
        }
    }
}