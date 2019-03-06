using UnityEngine;
using Panda;
using Sirenix.OdinInspector;


[RequireComponent(typeof(PandaBehaviour))]
public class AI : MonoBehaviour
{

    public EnemyTypes typeEnemy;

    private Transform target;

    [ShowIf("typeEnemy", EnemyTypes.Shooter)]
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

    [Title("Parametros")]
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


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pBT = GetComponent<PandaBehaviour>();
        anim = GetComponent<Animator>();
        target = GameObject.FindGameObjectWithTag("Player").transform;
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
    public bool isDashing()
    {
        return dash;
    }

    [Task]
    public bool isLookAtTag(string tag)
    {
        distance = target.position - transform.position;

        if (Physics.Raycast(transform.position, transform.forward.normalized, out hit, visionDistance))
        {
            if (hit.transform.CompareTag(tag))
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
    void WaitTimeInDash()
    {
        pBT.Wait(timeInDashing);
    }

    [Task]
    public void SetAnimatorTrigger(string animation)
    {
        anim.SetTrigger(animation);
        Task.current.Succeed();
    }

    [Task]
    public void WaitCurrentAnimationEnd()
    {
        pBT.Wait(anim.GetCurrentAnimatorStateInfo(0).length);
    }


    [Task]
    public void Dash(bool flag)
    {
        if (flag)
        {
            rb.velocity = transform.forward * dashForce;
        }

        dash = flag;

        Task.current.Succeed();

    }

    #endregion

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionDistance);
    }
}