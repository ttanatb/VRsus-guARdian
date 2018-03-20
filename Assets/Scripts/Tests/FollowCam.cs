using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCam : MonoBehaviour
{
    public GameObject followingObj;
    public Vector3 posOffset = new Vector3(1f, 2f, 1f);

    // Update is called once per frame
    void Update()
    {
        if (followingObj)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation,
            Quaternion.LookRotation(followingObj.transform.position - transform.position), 0.051f);
            transform.position = Vector3.Lerp(transform.position, followingObj.transform.position + posOffset, 0.01f);
        }
    }
}
