using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debug_CameraMovement : MonoBehaviour {

    public float movementSpeed = 4.0f;
    public float rotationSpeed = 2.0f;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Move();
        Rotate();
    }

    private void Move()
    {
        //Local movement combimation
        Vector3 move = Vector3.zero;

        //Move Forward / Back
        if (Input.GetKey(KeyCode.W))
        {
            move += transform.forward;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            move -= transform.forward;
        }

        //Move Left / Right
        if (Input.GetKey(KeyCode.A))
        {
            move -= transform.right;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            move += transform.right;
        }

        //Remove any movement in the Y direction and normalize
        move.y = 0;
        move.Normalize();

        if (Input.GetKey(KeyCode.LeftShift))
        {
            move.y = -1;
        } else if (Input.GetKey(KeyCode.Space))
        {
            move.y = 1;
        }

        //Move the camera
        transform.position += move * movementSpeed * Time.deltaTime;
    }

    private void Rotate()
    {
        //Make sure the right mouse button is down
        if (!Input.GetMouseButton(1)) { return; }

        //Get the X and Y rotation
        float x = Input.GetAxis("Mouse X") * rotationSpeed;
        float y = -Input.GetAxis("Mouse Y") * rotationSpeed;

        //Set the rotation of the camera
        transform.eulerAngles += new Vector3(y, x, 0);
    }
}
