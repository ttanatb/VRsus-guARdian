using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRAvatarRotationOffset : MonoBehaviour
{
    public Transform head;
    //private Vector3 offset;
    public Animator anim;
    private Vector3 pos;
    public Transform baseCollider;

    void Start()
    {
        //offset = (transform.position - head.position) * 2f;
        pos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        //transform.position = head.position + offset;
        transform.position = baseCollider.transform.position;
        anim.SetBool("isWalking", (Mathf.Abs(pos.x - transform.position.x) > 0f || Mathf.Abs(pos.z - transform.position.z) > 0f));
        transform.rotation = Quaternion.identity * Quaternion.AngleAxis(head.localRotation.eulerAngles.y, Vector3.up); // * transform.localRotation;

        pos = transform.position;
    }
}
