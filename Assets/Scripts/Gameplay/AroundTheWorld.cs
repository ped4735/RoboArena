using UnityEngine;

public class AroundTheWorld : MonoBehaviour
{

    //public Transform target;
    public float speedRotation;

    void Update()
    {
        //transform.RotateAround(target.position, Vector3.up, 1f);
        transform.Rotate(0, Time.deltaTime * speedRotation, 0);
    }
}
