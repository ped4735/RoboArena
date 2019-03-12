using UnityEngine;

public class FSM_Shoot_Navmesh : StateMachineBehaviour
{

    private NPC_Navmesh npc;
    private NPC_Shooter shooter;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        npc = animator.GetComponent<NPC_Navmesh>();
        shooter = animator.GetComponent<NPC_Shooter>();
        npc.GetNavMeshAgent().isStopped = true;
        shooter.StartShoot();
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Vector3 direction = npc.GetTarget().position - animator.transform.position;
        direction.y = 0;

        //Rotate
        animator.transform.forward = Vector3.Slerp(animator.transform.forward, direction, npc.rotationSpeed);

        
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        shooter.StopShoot();
    }

}
