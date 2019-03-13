using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableAfterTime : MonoBehaviour
{
    public float time;


    private void OnEnable()
    {
        StartCoroutine("Disable");
    }

    void Start()
    {
        StartCoroutine("Disable");
    }   

    IEnumerator Disable()
    {
        yield return new WaitForSeconds(time);
        gameObject.SetActive(false);
    }
}
