using UnityEngine;
using Panda;
using UnityEngine.AI;

public class AI_Task_Dash_Navmesh : MonoBehaviour
{
    private NavMeshAgent agent;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    [Task]
    public void Dash()
    {
        agent.velocity = agent.velocity.normalized * 10;
        agent.acceleration = 1;
        //agent.SetDestination(agent.destination + agent.destination.normalized * 5f);
        Task.current.Succeed();
    }


}
