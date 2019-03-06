using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LostParentAfterTime : MonoBehaviour
{

    public float time;

    void Start()
    {
        StartCoroutine("LostParentIn");
    }

    IEnumerator LostParentIn()
    {
        yield return new WaitForSeconds(time);
        transform.SetParent(transform.parent.parent);
    }
}
