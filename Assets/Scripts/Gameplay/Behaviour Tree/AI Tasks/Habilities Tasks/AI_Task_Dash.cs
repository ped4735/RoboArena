using UnityEngine;
using Panda;
using Sirenix.OdinInspector;

[RequireComponent(typeof(PandaBehaviour))]
public class AI_Task_Dash : MonoBehaviour
{
    private PandaBehaviour pBT;
    private Rigidbody rb;

    [Title("Dash Parameters")]
    public float dashForce;
    public float timeInDashing;
    private bool dash;


    private void Start()
    {
        pBT = GetComponent<PandaBehaviour>();
        rb = GetComponent<Rigidbody>();
    }

    [Task]
    public bool isDashing()
    {
        return dash;
    }

    [Task]
    void WaitTimeInDash()
    {
        pBT.Wait(timeInDashing);
    }

    [Task]
    public void Dash(bool flag)
    {
        if (flag)
        {
            rb.velocity = transform.forward * dashForce;
        }

        dash = flag;

        Task.current.Succeed();

    }
}
