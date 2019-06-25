using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerJoyControl : MonoBehaviour
{
    public PoolTypes bulletType;

    public Joystick joyL, joyR;
    public float moveSpeedWalk, moveSpeedShoot;
    public Transform weaponTipL, weaponTipR;


    private float horizontal, vertical;
    private Rigidbody rb;
    private bool waitForDash, waitForSecDash;
    private bool shooting, walking;
    private Animator anim;
    private JoyDash joyDash;

    //Upgrade 
    private int playerDamage = 10;
    private float playerAtackRange = 0.5f;
    private float fireRate = 1;
    private float moveSpeed;

    void Start()
    {
        moveSpeed = moveSpeedWalk;
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        joyDash = GetComponent<JoyDash>();
    }

    void Update()
    {
        float time = Time.deltaTime;

        horizontal = joyL.Horizontal;
        vertical = joyL.Vertical;

        Vector3 velocity = new Vector3(horizontal, rb.velocity.y, vertical).normalized;

        //rb.velocity = new Vector3(horizontal * moveSpeed * time, rb.velocity.y, vertical * moveSpeed * time);
        rb.velocity = velocity * moveSpeed * time;

      
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

        //Transforma o angulo do joy direito no angulo que que o player esta olhando.
        float angleR = Vector2.Angle(new Vector2(0, 1), joyR.Direction); //0° na pos (0,1) - joy pra cima.
        float angleL = Vector2.Angle(new Vector2(0, 1), joyL.Direction); //0° na pos (0,1) - joy pra cima.

        if (shooting)
        {
            moveSpeed = moveSpeedShoot;
            anim.speed = fireRate;

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
            anim.speed = 1;

            if (joyL.Horizontal > 0)
            {
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, angleL, transform.eulerAngles.z);
            }
            else if (joyL.Horizontal < 0)
            {
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, -angleL, transform.eulerAngles.z);
            }
        }

        //Set animation params
        anim.SetBool("walk", walking);
        anim.SetBool("shooting", shooting);
        anim.SetBool("dash", joyDash.dash);
    }

    //Shoot via event animation
    public void ShootL()
    {
        GameObject bullet = PoolController.instance.GetBullet(bulletType);
        bullet.GetComponent<DamageOnTouch>().SetDamage(playerDamage);
        bullet.GetComponent<Bullet>().SetDurationOfBullet(playerAtackRange);
        bullet.transform.position = weaponTipL.position;
        bullet.transform.rotation = transform.rotation;
        AudioManager.instance.PlayByID(2);
    }

    public void ShootR()
    {
        GameObject bullet = PoolController.instance.GetBullet(bulletType);
        bullet.GetComponent<DamageOnTouch>().SetDamage(playerDamage);
        bullet.GetComponent<Bullet>().SetDurationOfBullet(playerAtackRange);
        bullet.transform.position = weaponTipR.position;
        bullet.transform.rotation = transform.rotation;
        AudioManager.instance.PlayByID(2);
    }

    public void SetPlayerDamage(int damage)
    {
        playerDamage = damage;
    }

    public void SetPlayerRange(float range)
    {
        playerAtackRange = range;
    }

    public void SetPlayerAtkSpeedByAnim(float animSpeed)
    {
        //TODO fazer alteração atk speed
        fireRate = animSpeed;
    }

}
