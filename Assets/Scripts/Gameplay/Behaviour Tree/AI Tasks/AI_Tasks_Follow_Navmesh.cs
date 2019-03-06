using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Panda;
using UnityEngine.AI;
using Sirenix.OdinInspector;


[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(PandaBehaviour))]
public class AI_Tasks_Follow_Navmesh : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform target;

    [Title("Movement Parameters")]
    [Range(0.5f, 6f)]
    public float speed = 3f;

    [Range(0.01f, 1f)]
    public float smoothRotation = 0.05f;
    [Range(10f, 120f)]
    public float smoothRotationNavAgent = 100f;

    [Title("Follow Parameters")]
    [Range(2f, 15f)]
    public float visionDistance = 2.5f;

    [Range(2f, 15f)]
    public float toleranceAngleVision = 2.5f;


    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = speed;
        agent.angularSpeed = smoothRotationNavAgent;
        agent.stoppingDistance = visionDistance;

        target = GameObject.FindGameObjectWithTag("Player").transform;
    }

    [Task]
    public void SetTargetByTag(string tag)
    {
        target = GameObject.FindGameObjectWithTag(tag).transform;
        Task.current.Succeed();
    }

    [Task]
    public void SetDestinationToTarget()
    {
        agent.SetDestination(target.position);
        Task.current.Succeed();
    }

    [Task]
    public void MoveDestinationToTarget()
    {
   
        if (Task.isInspected)
            Task.current.debugInfo = string.Format("d={0}", agent.remainingDistance);

        if (agent.remainingDistance <= visionDistance && !agent.pathPending)
        {
            Task.current.Succeed();
        }
    }

    [Task]
    public void Stop(bool stop)
    {
        agent.isStopped = stop;
        Task.current.Succeed();
    }

    [Task]
    public bool IsReachTheTarget()
    {
        if (agent.remainingDistance <= visionDistance && !agent.pathPending)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    [Task]
    public bool IsReachTheTargetDistance()
    {
        Vector3 distance = transform.position - target.position;
        distance.y = 0;

        if(distance.magnitude <= visionDistance)
        {
            return true;
        }
        else
        {
            return false;
        }

    }

    [Task]
    public void RotateToFaceTarget()
    {
        Vector3 distance = target.position - transform.position;
        distance.y = 0;

        transform.forward = Vector3.Slerp(transform.forward, distance, smoothRotation);
        Task.current.Succeed();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionDistance);

        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, transform.forward.normalized * visionDistance);
    }

}
