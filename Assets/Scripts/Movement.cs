using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Movement : NetworkBehaviour
{
    Rigidbody rigidBody;
    float jumpValue = 0;
    float lerpTarget = 0;

    const float JUMP_FACTOR = 2;
    const float JUMP_DAMPING = 0.2f;

    const float MOVEMENT_SPEED = 0.025f;
    const float TURNING_SPEED = 5f;

    // Use this for initialization
    void Start()
    {
        rigidBody = GetComponentInChildren<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer)
            return;

        transform.Translate(transform.forward * Input.GetAxis("Vertical") * MOVEMENT_SPEED, Space.World);
        transform.Rotate(transform.up, Input.GetAxis("Horizontal") * TURNING_SPEED, Space.World);

        if (Input.GetKeyDown(KeyCode.Space))
            lerpTarget = JUMP_FACTOR;

        jumpValue = Mathf.Lerp(jumpValue, lerpTarget, Time.deltaTime * 20);

        if (lerpTarget > 0)
            lerpTarget -= JUMP_DAMPING;
        else lerpTarget = 0;
    }

    private void FixedUpdate()
    {
        rigidBody.velocity += Vector3.up * jumpValue;
    }
}
