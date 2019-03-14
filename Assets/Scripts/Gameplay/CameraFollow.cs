using UnityEngine;

public class CameraFollow : MonoBehaviour {

	public Transform target;

    [Range(0.01f, 1f)]
	public float smoothSpeed = 0.5f;
	public Vector3 offset;

    public bool lookAtPlayer;

    private void Start()
    {
        //offset = transform.position - target.position;
        transform.position = target.position + offset;
    }

    void FixedUpdate ()
	{
		Vector3 desiredPosition = target.position + offset;
		Vector3 smoothedPosition = Vector3.Slerp(transform.position, desiredPosition, smoothSpeed);
		transform.position = smoothedPosition;

        if (lookAtPlayer)
        {
            transform.LookAt(target.position);
        }

    }

}
