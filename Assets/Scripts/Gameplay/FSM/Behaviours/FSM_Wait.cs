using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;


/*
 * 
 * Behaviour que gera uma espera no estado do animator. 
 * Tambem seleciona de forma aleatoria entre os triggers listados ao finalizar o tempo de espera.
 * 
 */
public class FSM_Wait : StateMachineBehaviour
{

    public List<string> triggers = new List<string>();


    public bool randomTime;
    [HideIf("randomTime")]
    public float timeToWait;

    [ShowIf("randomTime")]
    public float timeToWait1;
    [ShowIf("randomTime")]
    public float timeToWait2;

    private float time;
    private string triggerSelected;
    private float countTime;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (randomTime)
            time = Random.Range(timeToWait1, timeToWait2);
        else
            time = timeToWait;

        triggerSelected = triggers[Random.Range(0, triggers.Count)];
        countTime = 0;
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        
        if(countTime < time)
        {
            countTime += Time.deltaTime;
        }
        else
        { 
            animator.SetTrigger(triggerSelected);
        }

    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        countTime = 0;
        animator.ResetTrigger(triggerSelected);
    }

}
