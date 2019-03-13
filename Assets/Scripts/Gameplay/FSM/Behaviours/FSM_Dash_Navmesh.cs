using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSM_Dash_Navmesh : StateMachineBehaviour
{

    private NPC_Navmesh npc;
    //private Transform refPoint;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        npc = animator.GetComponent<NPC_Navmesh>();

        Vector3 direction = npc.GetTarget().position - animator.transform.position;
        direction.y = 0;

        Vector3 destination = npc.GetTarget().position + direction.normalized * 5f;


        npc.GetNavMeshAgent().velocity = animator.transform.forward * 30f;
        npc.GetNavMeshAgent().SetDestination(destination);
        npc.GetNavMeshAgent().isStopped = false;

        //Debug.Log("Dash Destination:" + destination);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        

        if(npc.GetNavMeshAgent().remainingDistance <= 1f)
        {
            animator.SetTrigger("chase");
        }


    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    npc.GetNavMeshAgent().acceleration = 8f;
    //}

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
