using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerJoyControl : MonoBehaviour
{
    public Pool pool;

    public Joystick joyL, joyR;
    public float moveSpeed, moveSpeedWalk, moveSpeedShoot;
    public Transform weaponTipL, weaponTipR;
    public float fireRate;

    private float horizontal, vertical;
    private Rigidbody rb;
    private bool waitForDash, waitForSecDash;
    private bool shooting, walking;
    private Animator anim;
    private JoyDash joyDash;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        joyDash = GetComponent<JoyDash>();
    }

    void Update()
    {
        float time = Time.deltaTime;

        horizontal = joyL.Horizontal;
        vertical = joyL.Vertical;

        rb.velocity = new Vector3(horizontal * moveSpeed * time, rb.velocity.y, vertical * moveSpeed * time);

      
        if (joyL.Direction != Vector2.zero)
        {
            walking = true;
        }
        else if (joyL.Direction == Vector2.zero)
        {
            walking = false;
        }
        

        
        if(joyR.Direction != Vector2.zero) //&& !shooting)
        {
            walking = false;
            shooting = true;
            //StartCoroutine(Fire());
        }
        else if (joyR.Direction == Vector2.zero)
        {
            shooting = false;
        }

        //Debug.Log("Walk:" + walking + " Shoot: " + shooting);
        //Transforma o angulo do joy direito no angulo que que o player esta olhando.
        float angleR = Vector2.Angle(new Vector2(0, 1), joyR.Direction); //0° na pos (0,1) - joy pra cima.
        float angleL = Vector2.Angle(new Vector2(0, 1), joyL.Direction); //0° na pos (0,1) - joy pra cima.

        if (shooting)
        {
            moveSpeed = moveSpeedShoot;
            if (joyR.Horizontal > 0)
            {
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, angleR, transform.eulerAngles.z);
            }
            else if (joyR.Horizontal < 0)
            {
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, -angleR, transform.eulerAngles.z);
            }
        }
        else
        {
            moveSpeed = moveSpeedWalk;

            if (joyL.Horizontal > 0)
            {
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, angleL, transform.eulerAngles.z);
            }
            else if (joyL.Horizontal < 0)
            {
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, -angleL, transform.eulerAngles.z);
            }
        }

        anim.SetBool("walk", walking);
        anim.SetBool("shooting", shooting);
        anim.SetBool("dash", joyDash.dash);
    }

    //Shoot via fireRate
    IEnumerator Fire()
    {        
        while (joyR.Direction != Vector2.zero)
        {
            var bullet = pool.nextThing;
            bullet.transform.position = weaponTipR.position;
            bullet.transform.rotation = transform.rotation;

            yield return new WaitForSeconds(fireRate);
        }
        shooting = false;
    }


    //Shoot via event animation
    public void ShootL()
    {
        var bullet = pool.nextThing;
        bullet.transform.position = weaponTipL.position;
        bullet.transform.rotation = transform.rotation;
    }

    public void ShootR()
    {
        var bullet = pool.nextThing;
        bullet.transform.position = weaponTipR.position;
        bullet.transform.rotation = transform.rotation;
    }

}
