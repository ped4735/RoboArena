using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Panda;


[RequireComponent(typeof(PandaBehaviour))]
public class AI_Tasks_AvoidObjects : MonoBehaviour
{
    public float turnSpeed = 0.05f;
    public float avoidDistance = 5f;

    private Vector3 vectToTurn;
    private RaycastHit hit;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    [Task]
    public void TurnInDegrees(float angle)
    {
            vectToTurn = Quaternion.AngleAxis(angle, Vector3.up) * transform.forward;
            transform.forward = vectToTurn;//Vector3.Slerp(transform.forward, vectToTurn, turnSpeed);
            Task.current.Succeed();
    }

    [Task]
    public bool IsLookToObstacle(string tag)
    {
        if(Physics.Raycast(transform.position, transform.forward, out hit, avoidDistance)){
            if (hit.transform.CompareTag(tag))
            {
                return true;
            }
        }

        return false;
    }
    
}
