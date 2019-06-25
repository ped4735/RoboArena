using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class FSM_LookTarget_Navmesh : StateMachineBehaviour
{

    private NPC_Navmesh npc;
    public string trigger;

    
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        npc = animator.GetComponent<NPC_Navmesh>();
    }

   
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Vector3 direction = npc.GetTarget().position - animator.transform.position;
        direction.y = 0;

        float angle = Vector3.Angle(animator.transform.forward, direction);

        //Rotate
        animator.transform.forward = Vector3.Slerp(animator.transform.forward, direction, npc.rotationSpeed);

        if(angle < npc.visionAngle)
        {
            animator.SetTrigger(trigger);
        }

    }

    
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    { 
       animator.ResetTrigger(trigger);
    }

}
