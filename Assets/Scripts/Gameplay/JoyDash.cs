using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoyDash : MonoBehaviour
{
    private Rigidbody rb;

    private bool waitingDash, alreadyClick;

    [HideInInspector]
    public bool dash;

    public Joystick Joy;
    public float timeToActivateDash;
    public float timeInDashing;


    private Vector2 dashDirection;

    public float dashForce;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {

        //alterei o joystick primeira vez
        if(Joy.Direction != Vector2.zero && !alreadyClick)
        {
            if (!waitingDash)
            {
                waitingDash = true;
                alreadyClick = true;
                StartCoroutine(DashActivateTiming());
            }
            else{ //DO DASH!
                alreadyClick = true;
                dashDirection = Joy.Direction;
                dashDirection = dashDirection.normalized;
                StartCoroutine(DashTiming());
            }
            
        }

        //Larguei o joystick
        if(Joy.Direction == Vector2.zero && alreadyClick)
        {
            alreadyClick = false;
        }


    }

    IEnumerator DashTiming()
    {
        dash = true;
        StartCoroutine(Dash());
        yield return new WaitForSeconds(timeInDashing);
        dash = false;
    }

    IEnumerator Dash()
    {
        

        while (dash)
        {
            rb.velocity = new Vector3(dashDirection.x,0,dashDirection.y) * dashForce;
            
            //rb.velocity = transform.forward * dashForce;
            yield return new WaitForEndOfFrame();
        }
        
    }

    IEnumerator DashActivateTiming()
    {
        
        yield return new WaitForSeconds(timeToActivateDash);
        //Debug.Log("Acabou tempo para Dash!");
        waitingDash = false;
    }
    

}
