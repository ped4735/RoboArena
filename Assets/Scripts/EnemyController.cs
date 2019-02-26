using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private Transform player;
    public bool followPlayer;

    [Range(0.01f, 1f)]
    public float smoothRotation = 0.5f;



    void Start()
    {
        player = GameObject.Find("Player").transform;
    }

    void Update()
    {
        if (followPlayer)
        {
            //Inimigo rotaciona para olhar para o player
            Vector3 desiredVector = player.position - transform.position;
            transform.forward = Vector3.Slerp(transform.forward, desiredVector, smoothRotation);

            //Movimento do inimigo para o player
        }
    }
}