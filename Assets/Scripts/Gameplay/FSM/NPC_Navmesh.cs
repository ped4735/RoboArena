using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NPC_Navmesh : MonoBehaviour
{
    private Transform target;
    private NavMeshAgent agent;
    private Animator anim;

    public float speed;
    public float rotationSpeed;
    public float visionRange;


    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        target = GameObject.FindGameObjectWithTag("Player").transform;
        anim = GetComponent<Animator>();

        agent.speed = speed;
    }

    private void Update()
    {
        anim.SetFloat("distance", Vector3.Distance(transform.position, target.position));
    }

    public NavMeshAgent GetNavMeshAgent()
    {
        return agent;
    }

    public Transform GetTarget()
    {
        return target;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, visionRange);
    }

}
