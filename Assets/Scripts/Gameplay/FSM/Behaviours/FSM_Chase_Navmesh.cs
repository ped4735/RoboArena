using UnityEngine;

public class FSM_Chase_Navmesh : StateMachineBehaviour
{

    private NPC_Navmesh npc;
    public bool rangeIsLower;
    public bool chase;
    public string trigger;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        npc = animator.GetComponent<NPC_Navmesh>();
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
         
        if(chase)
            npc.GetNavMeshAgent().SetDestination(npc.GetTarget().position);


        float distance = Vector3.Distance(animator.transform.position, npc.GetTarget().position);

        if (rangeIsLower)
        {
            if (distance < npc.visionRange)
            {
                animator.SetTrigger(trigger);
            }
        }
        else
        {
            if (distance > npc.visionRange)
            {
                animator.SetTrigger(trigger);
            }
        }

    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.ResetTrigger(trigger);
    }

}
