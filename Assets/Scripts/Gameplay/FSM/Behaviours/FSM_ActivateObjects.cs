using UnityEngine;
using Sirenix.OdinInspector;

/*
 * 
 * Behaviour que ativa e desativa Gameobjects referenciados pelo monobehaviour com o componente<NPC_ActivateObjectsHandler>.
 * 
 */
public class FSM_ActivateObjects : StateMachineBehaviour
{

    private NPC_ActivateObjectsHandler objectsHandler;
    public int groupID;
    public bool activateObjects;
    [ShowIf("activateObjects")]
    public bool deactivateOnExit;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        objectsHandler = animator.GetComponent<NPC_ActivateObjectsHandler>();

        for (int i = 0; i < objectsHandler.objectGroup[groupID].objects.Count; i++)
        {
            objectsHandler.objectGroup[groupID].objects[i].SetActive(activateObjects);
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (deactivateOnExit)
        {
            for (int i = 0; i < objectsHandler.objectGroup[groupID].objects.Count; i++)
            {
                objectsHandler.objectGroup[groupID].objects[i].SetActive(false);
            }
        }
        
    }

}
