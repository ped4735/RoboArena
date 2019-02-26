/*
 * Credit to:
 * http://blog.boredmormongames.com/2014/08/object-pooling.html
 */

using UnityEngine;
using System.Collections;

public class PoolMember : MonoBehaviour
    {
        public Pool pool;

        void OnDisable()
        {  
            pool.nextThing = gameObject;  
        }
}