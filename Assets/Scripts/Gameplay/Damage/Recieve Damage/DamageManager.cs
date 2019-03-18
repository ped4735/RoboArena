using UnityEngine;

public abstract class DamageManager : MonoBehaviour
{ 
    public abstract void Hit(int damage);
    public abstract void Death();
}
