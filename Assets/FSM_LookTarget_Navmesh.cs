using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSM_LookTarget_Navmesh : StateMachineBehaviour
{

    private NPC_Navmesh npc;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        npc = animator.GetComponent<NPC_Navmesh>();

        npc.GetNavMeshAgent().isStopped = true;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Vector3 direction = npc.GetTarget().position - animator.transform.position;
        direction.y = 0;

        float angle = Vector3.Angle(animator.transform.forward, direction);

        //Rotate
        animator.transform.forward = Vector3.Slerp(animator.transform.forward, direction, npc.rotationSpeed);

        if(angle < npc.visionAngle)
        {
            animator.SetTrigger("preparation");
        }

    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

}
