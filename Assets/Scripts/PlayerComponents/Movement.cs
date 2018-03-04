using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.VR;

//FPS camera movement adapted from: http://wiki.unity3d.com/index.php/SmoothMouseLook
public class Movement : PlayerComponent
{
    public float movementSpeed = 0.025f;
    public float sprintFactor = 3f;
    public float maxSpeed = 5f;

    private bool isPlaying = true;

    public float mouseSensitivity = 100.0f;
    public float clampAngle = 80.0f;

    private float rotY = 0.0f; // rotation around the up/y axis
    private float rotX = 0.0f; // rotation around the right/x axis

    private Vector3 startingPos;

    public float slowFactor = 0.3f;
    private float slowTimer;
    private const float MAX_SLOW_TIME = 2f;

    public bool isOnFloor = true;

    private Rigidbody rBody;
    private TrailRenderer trailRenderer;
    public GameObject leftController;
    public GameObject rightController;

    public GameObject grapplePrefab;

    private Animator networkAnimator;

    public Transform cameraTransform;

    public bool IsSlowed
    {
        get { return slowTimer > MAX_SLOW_TIME / 2f; }
    }

    // Use this for initialization
    void Awake()
    {
        startingPos = transform.position;
        Vector3 rot = transform.localRotation.eulerAngles;
        rotY = rot.y;
        rotX = rot.x;
    }

    protected override void InitObj()
    {
        if (!trailRenderer && playerType != PlayerType.AR)
            trailRenderer = GetComponentInChildren<TrailRenderer>();

        rBody = GetComponent<Rigidbody>();
    }

    void Start()
    {
        isPlaying = true;
        InitObj();
        networkAnimator = GetComponentInChildren<Animator>();

        if (!cameraTransform) cameraTransform = transform;

        if ((playerType == PlayerType.PC || playerType == PlayerType.VR) && isLocalPlayer)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Grappling left = Instantiate(grapplePrefab).GetComponent<Grappling>();
            Grappling right = Instantiate(grapplePrefab).GetComponent<Grappling>();

            left.player = gameObject;
            right.player = gameObject;
            left.playerAnchor = leftController.transform;
            right.playerAnchor = rightController.transform;
            left.animController = networkAnimator;
            right.animController = networkAnimator;
            left.button = "Fire2";
            right.button = "Fire3";
            left.otherGrappling = right;
            right.otherGrappling = left;
            if (playerType == PlayerType.PC)
            {
                left.spawnPos = transform;
                right.spawnPos = transform;
            }
            else
            {
                left.spawnPos  = leftController.transform;
                right.spawnPos = rightController.transform;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer) return;
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.G))
        {
            transform.position = startingPos;
        }

        slowTimer -= Time.deltaTime;
        if (playerType != PlayerType.VR)
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = -Input.GetAxis("Mouse Y");

#if !UNITY_EDITOR
            rotY += mouseX * mouseSensitivity * Time.deltaTime * 10f;
            rotX += mouseY * mouseSensitivity * Time.deltaTime * 10f;
#else
            rotY += mouseX * mouseSensitivity * Time.deltaTime;
            rotX += mouseY * mouseSensitivity * Time.deltaTime;
#endif
            rotX = Mathf.Clamp(rotX, -clampAngle, clampAngle);

            transform.rotation = Quaternion.AngleAxis(rotY, Vector3.up);
            cameraTransform.rotation = transform.rotation * Quaternion.AngleAxis(rotX, Vector3.right) ; // 0.0f, rotY, 0.0f);
        }
    }

    private void FixedUpdate()
    {
        if (!isLocalPlayer) return;

        if (isPlaying)
        {
            if (playerType == PlayerType.VR)
            {
                Vector3 movementX = Vector3.zero;
                Vector3 movementY = Vector3.zero;

                if (Input.GetAxis("VRLeftHorizontal") != 0 || Input.GetAxis("VRLeftVertical") != 0)
                {
                    movementX = leftController.transform.right * Input.GetAxis("VRLeftHorizontal") * movementSpeed;

                    movementY = (leftController.transform.forward * Input.GetAxis("VRLeftVertical")) * movementSpeed;
                }
                else if (Input.GetAxis("VRRightHorizontal") != 0 || Input.GetAxis("VRRightVertical") != 0)
                {
                    movementX = rightController.transform.right * Input.GetAxis("VRRightHorizontal") * movementSpeed;

                    movementY = (rightController.transform.forward * Input.GetAxis("VRRightVertical")) * movementSpeed;
                }

                if (slowTimer > 0f)
                {
                    movementX *= Mathf.Lerp(1.0f, slowFactor, slowTimer / MAX_SLOW_TIME);
                    movementY *= Mathf.Lerp(1.0f, slowFactor, slowTimer / MAX_SLOW_TIME);
                }

                rBody.MovePosition(movementX + movementY + transform.position);
            }
            else
            {
                Vector3 movement = cameraTransform.right * Input.GetAxis("Horizontal");

                if (playerType == PlayerType.AR)
                {
                    movement += Input.GetAxis("UpDown") * Vector3.up;
                    movement = (movement + cameraTransform.forward * Input.GetAxis("Vertical")) * movementSpeed;
                }
                else
                {
                    movement = (movement + Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up) * Input.GetAxis("Vertical")) * movementSpeed;
                }

                //Debug.Log("Timer: " + slowTimer);
                if (slowTimer > 0f)
                {
                    movement *= Mathf.Lerp(1.0f, slowFactor, slowTimer / MAX_SLOW_TIME);
                }

                if (playerType == PlayerType.PC)
                {
                    networkAnimator.SetBool("isWalking", (Mathf.Abs(movement.x) > 0f || Mathf.Abs(movement.z) > 0f));
                    rBody.AddForce(movement, ForceMode.Acceleration);
                }
                else
                {
                    Vector3 newPos = transform.position + movement;
                    transform.position = newPos;
                }

                if (Input.GetButton("Jump")) Jump();
            }
        }
    }

    public void DisableMovement()
    {
        isPlaying = false;
    }

    public void EnableMovement()
    {
        isPlaying = true;
    }

    public void Jump()
    {
        if (isOnFloor)
        {
            rBody.AddForce(Vector3.Lerp(Vector3.up * 2f, Vector3.zero, slowTimer / MAX_SLOW_TIME), ForceMode.VelocityChange);
            isOnFloor = false;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if ((collision.gameObject.tag == "Platform") &&
            (transform.position.y > collision.transform.position.y + collision.transform.localScale.y / 2f))
        {
            networkAnimator.SetBool("isOnLand", true);
            isOnFloor = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Platform")
        {
            networkAnimator.SetBool("isOnLand", false);
            isOnFloor = false;
        }
    }

    //[Command]
    //private void CmdTurnOnTrailRenderer()
    //{
    //    if (isLocalPlayer) return;

    //    if (!trailRenderer)
    //    {
    //        trailRenderer = GetComponentInChildren<TrailRenderer>();
    //    }

    //    if (trailRenderer)
    //        trailRenderer.time = 1f;
    //}

    //[Command]
    //private void CmdTurnOffTrailRenderer()
    //{
    //    if (isLocalPlayer) return;

    //    if (!trailRenderer)
    //    {
    //        trailRenderer = GetComponentInChildren<TrailRenderer>();
    //    }

    //    if (trailRenderer)
    //        trailRenderer.time = 0f;
    //}

    [ClientRpc]
    public void RpcSlow()
    {
        slowTimer = MAX_SLOW_TIME;
    }
}