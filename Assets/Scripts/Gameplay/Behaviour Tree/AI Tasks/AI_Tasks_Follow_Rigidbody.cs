using UnityEngine;
using Panda;
using Sirenix.OdinInspector;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PandaBehaviour))]
public class AI_Tasks_Follow_Rigidbody : MonoBehaviour
{
    private Transform target;
    private Vector3 distance;
    private Rigidbody rb;

    [Title("Movement Parameters")]
    [Range(0.5f, 6f)]
    public float speed = 3f;

    [Range(0.01f, 1f)]
    public float smoothRotation = 0.5f;

    [Title("Follow Parameters")]
    [Range(2f, 15f)]
    public float visionDistance = 2.5f;

    [Range(2f, 15f)]
    public float toleranceAngleVision = 2.5f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    [Task]
    public void SetTargetByTag(string tag)
    {
        target = GameObject.FindGameObjectWithTag(tag).transform;
        Task.current.Succeed();
    }

    [Task]
    public void MoveForward()
    {
        rb.velocity = transform.forward.normalized * speed;
        Task.current.Succeed();
    }

    [Task]
    public void RotateToFaceTarget()
    {
        distance = target.position - transform.position;
        distance.y = 0;

        //Inimigo rotaciona para olhar para o target
        transform.forward = Vector3.Slerp(transform.forward, distance, smoothRotation);

        Task.current.Succeed();
    }

    [Task]
    public void Stop()
    {
        rb.velocity = Vector3.zero;
        Task.current.Succeed();
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

   
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionDistance);
    }
}
