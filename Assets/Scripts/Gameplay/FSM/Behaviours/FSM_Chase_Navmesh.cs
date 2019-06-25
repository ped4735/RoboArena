using UnityEngine;
using Sirenix.OdinInspector;



/*
 * 
 * Behaviour que da chase ou verifica distancia para ativar trigger.
 * 
 */

public class FSM_Chase_Navmesh : StateMachineBehaviour
{
    private NPC_Navmesh npc;
    public bool rangeIsLower;
    public bool chase;
    public string trigger;

    public bool useCustomRange;
    [ShowIf("useCustomRange")]
    public float range;

   
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        npc = animator.GetComponent<NPC_Navmesh>();
        
    }

    
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
         
        if(chase)
            npc.GetNavMeshAgent().SetDestination(npc.GetTarget().position);


        float distance = Vector3.Distance(animator.transform.position, npc.GetTarget().position);

        if (rangeIsLower)
        {
            if (useCustomRange)
            {
                if (distance < range)
                {
                    animator.SetTrigger(trigger);
                }
            }
            else
            {
                if (distance < npc.visionRange)
                {
                    animator.SetTrigger(trigger);
                }
            }
            
        }
        else
        {
            if (useCustomRange)
            {
                if (distance > range)
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

    }


    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.ResetTrigger(trigger);
    }

}
