using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debug_CameraMovement : MonoBehaviour {

    public float movementSpeed = 3.0f;
    public float rotationSpeed = 2.0f;

    public KeyCode forward = KeyCode.W;
    public KeyCode backward = KeyCode.S;
    public KeyCode left = KeyCode.A;
    public KeyCode right = KeyCode.D;

    public KeyCode up = KeyCode.LeftShift;
    public KeyCode down = KeyCode.Space;

    public int mouseDrag = 1;
    private bool debugging = false;

    // Use this for initialization
    void Start()
    {
        GetComponent<Camera>().enabled = debugging;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            GetComponent<Camera>().enabled = !GetComponent<Camera>().enabled;
            debugging = !debugging;
        }

        if (!debugging) { return; }

        Move();
        Rotate();
    }

    /// <summary>
    /// Move the camera relative to the direction the camera is rotated
    /// </summary>
    private void Move()
    {
        //Local movement combimation
        Vector3 move = Vector3.zero;

        //Move Forward / Back
        if (Input.GetKey(forward))
        {
            move += transform.forward;
        }
        else if (Input.GetKey(backward))
        {
            move -= transform.forward;
        }

        //Move Left / Right
        if (Input.GetKey(left))
        {
            move -= transform.right;
        }
        else if (Input.GetKey(right))
        {
            move += transform.right;
        }

        //Remove any movement in the Y direction and normalize
        move.y = 0;
        move.Normalize();

        if (Input.GetKey(up))
        {
            move.y = -1;
        }
        else if (Input.GetKey(down))
        {
            move.y = 1;
        }

        //Move the camera
        transform.position += move * movementSpeed * Time.deltaTime;
    }

    /// <summary>
    /// Rotate the camera on the X and Y axis
    /// </summary>
    private void Rotate()
    {
        //Make sure the right mouse button is down
        if (!Input.GetMouseButton(mouseDrag)) { return; }

        //Get the X and Y rotation
        float x = Input.GetAxis("Mouse X") * rotationSpeed;
        float y = -Input.GetAxis("Mouse Y") * rotationSpeed;

        //Set the rotation of the camera
        transform.eulerAngles += new Vector3(y, x, 0);
    }
}
