using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Panda;


public class AI : MonoBehaviour
{

    public Transform target;
    public Pool pool;
    public Transform aim;

    [Range(0.01f, 2f)]
    public float fireRate;

    [Range(0.5f, 6f)]
    public float speed = 3f;

    [Range(0.01f, 1f)]
    public float smoothRotation = 0.5f;

    [Range(2f, 15f)]
    public float visionDistance = 2.5f;

    private Vector3 distance;
    private Rigidbody rb;
    private bool shooting;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    [Task]
    public void FollowTarget()
    {

        distance = target.position - transform.position;
        distance.y = 0;

        //Movimento do inimigo para o player
        transform.Translate(0, 0, Time.deltaTime * speed);

        Task.current.Succeed();
    }

    [Task]
    public void RotateToFaceTarget()
    {
        distance = target.position - transform.position;
        distance.y = 0;

        //Inimigo rotaciona para olhar para o player
        transform.forward = Vector3.Slerp(transform.forward, distance, smoothRotation);

        Task.current.Succeed();
    }

    [Task]
    public bool LookToTarget()
    {
        distance = target.position - transform.position;
        distance.y = 0;

        if (Vector3.Angle(transform.forward, distance) < 2f) 
        {
            return true;
        }

        return false;       
    }

    [Task]
    public bool ReachTargetAtDistance()
    {
        distance = target.position - transform.position;

        if (distance.magnitude < visionDistance)
        {
            return true;
        }

        return false;

    }

    [Task]
    public void Log(string msg)
    {
        Debug.Log(msg);
        Task.current.Succeed();
    }

    [Task]
    public void Dash()
    {

    }

    [Task]
    public void Shoot(bool flag)
    {
                
        if (flag && !shooting)
        {
            shooting = true;
            StartCoroutine("Shooting");
            Task.current.Succeed();
        }
        
        if(!flag)
        {
            shooting = false;
            StopCoroutine("Shooting");
        }

        Task.current.Succeed();
    }


    IEnumerator Shooting()
    {
        while (shooting)
        {
            var bullet = pool.nextThing;
            bullet.transform.position = aim.position;
            bullet.transform.rotation = transform.rotation;

            yield return new WaitForSeconds(fireRate);
        }
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionDistance);
    }
}
