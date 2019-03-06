using UnityEngine;
using Panda;
using Sirenix.OdinInspector;

[RequireComponent(typeof(PandaBehaviour))]
[RequireComponent(typeof(Animator))]
public class AI_Tasks_Animation : MonoBehaviour
{
    private Animator anim;
    private PandaBehaviour pBT;

    private void Start()
    {
        pBT = GetComponent<PandaBehaviour>();
        anim = GetComponent<Animator>();
    }
    
    [Task]
    public void SetAnimatorTrigger(string animation)
    {
        anim.SetTrigger(animation);
        Task.current.Succeed();
    }

    [Task]
    public void WaitCurrentAnimationEnd()
    {
        pBT.Wait(anim.GetCurrentAnimatorStateInfo(0).length);
    }
    
}
