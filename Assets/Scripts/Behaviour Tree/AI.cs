using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Panda;
using Sirenix.OdinInspector;


[RequireComponent(typeof(PandaBehaviour))]
[HideMonoScript]
public class AI : MonoBehaviour
{

    public EnemyTypes typeEnemy;

    [SerializeField]
    public Transform target;

    [ShowIf("typeEnemy",EnemyTypes.Shooter)]
    public Pool pool;
    [ShowIf("typeEnemy", EnemyTypes.Shooter)]
    public Transform aim;
    [ShowIf("typeEnemy", EnemyTypes.Shooter)]
    [Range(0.01f, 2f)]
    public float fireRate;

    [ShowIf("typeEnemy", EnemyTypes.Dasher)]
    public float dashForce;
    [ShowIf("typeEnemy", EnemyTypes.Dasher)]
    public float timeInDashing;
    private bool dash;

    [Range(0.5f, 6f)]
    public float speed = 3f;

    [Range(0.01f, 1f)]
    public float smoothRotation = 0.5f;

    [Range(2f, 15f)]
    public float visionDistance = 2.5f;

    [Range(2f, 15f)]
    public float toleranceAngleVision = 2.5f;

    private Vector3 distance;
    private Rigidbody rb;
    private bool shooting;
    private PandaBehaviour pBT;
    private RaycastHit hit;
    private Animator anim;

    public float time;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pBT = GetComponent<PandaBehaviour>();
        anim = GetComponent<Animator>();  
    }

    #region Panda Tasks
    [Task]
    public void Stop()
    {
        rb.velocity = Vector3.zero;
        Task.current.Succeed();
    }

    [Task]
    public void FollowTargetRb()
    {

        distance = target.position - transform.position;
        distance.y = 0;

        //Movimento do inimigo para o player
        //transform.Translate(0, 0, Time.deltaTime * speed);
        rb.velocity = transform.forward.normalized * speed;


        Task.current.Succeed();
    }

    [Task]
    public void FollowTargetT()
    {

        distance = target.position - transform.position;
        distance.y = 0;

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
    public bool isLookToTarget()
    {
        distance = target.position - transform.position;
        distance.y = 0;

        if (Vector3.Angle(transform.forward, distance) < toleranceAngleVision) 
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
        if (!dash)
        {
            Debug.Log("Dash!");
            StartCoroutine("DashTiming");
        }
        Task.current.Succeed();
    }

    [Task]
    public bool isDashing()
    {
        return dash;
    }

    [Task]
    public bool isLookAtObstacle()
    {
        distance = target.position - transform.position;

        //if (Physics.Raycast(transform.position, distance, out hit, visionDistance))
        //{
        //    if (hit.transform.CompareTag("Obstacle"))
        //    {
        //        return true;
        //    }
        //}

        if (Physics.Raycast(transform.position, transform.forward.normalized, out hit, visionDistance))
        {
            if (hit.transform.CompareTag("Obstacle"))
            {
                return true;
            }
        }

        return false;
    }

    [Task]
    public void Shoot()
    {
        var bullet = pool.nextThing;
        bullet.transform.position = aim.position;
        bullet.transform.rotation = transform.rotation;

        Task.current.Succeed();
    }

    [Task]
    void WaitAttackDelay()
    {
        pBT.Wait(fireRate);
    }

    [Task]
    public void SetAnimatorTrigger(string animation)
    {
        anim.SetTrigger(animation);
        Task.current.Succeed();
    }

    [Task]
    public void Dash2(bool flag)
    {
        if (flag)
        {
            rb.velocity = transform.forward * dashForce;
        }            
        

        dash = flag;

        Task.current.Succeed();
            
    }

    #endregion

    IEnumerator DashTiming()
    {
        dash = true;
        StartCoroutine(Dashing());
        yield return new WaitForSeconds(timeInDashing);
        dash = false;
    }

    IEnumerator Dashing()
    {
        while (dash)
        {
            rb.velocity = transform.forward * dashForce;
            yield return new WaitForEndOfFrame();
        }

    }

    private void OnDrawGizmosSelected()
    {
        distance = target.position - transform.position;
        distance.y = 0;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionDistance);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, target.position);
    }
}

public enum EnemyTypes
{
    Shooter,
    Dasher
}
