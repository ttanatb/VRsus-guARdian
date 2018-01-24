using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.VR;

//FPS camera movement adapted from: http://wiki.unity3d.com/index.php/SmoothMouseLook
public class TestPlayerMovement : PlayerComponent
{
    private Rigidbody rigidBody;

    public float jumpCost = 3f;
    public float jumpEnergyMax = 15f;
    public float energyRegainRate = 5f;
    private float currJumpEnergy;

    public float jumpFactor = 2;
    public float movementSpeed = 0.025f;
    public float sprintFactor = 3f;

    public float maxSpeed = 5f;

    private bool isPlaying = false;

    public float mouseSensitivity = 100.0f;
    public float clampAngle = 80.0f;

    private float rotY = 0.0f; // rotation around the up/y axis
    private float rotX = 0.0f; // rotation around the right/x axis

    public uint frameCounter = 20;
    Vector3 startingPos;

    [SyncVar]
    bool isSlowed = false;
    public float slowFactor = 0.7f;
    TrailRenderer trailRenderer;

    public bool isOnFloor = false;

    public bool IsSlowed
    {
        get { return isSlowed; }
        set { isSlowed = value; }
    }

    public float CurrJumpEnergy { get { return currJumpEnergy; } }

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
        if (!trailRenderer)
            trailRenderer = GetComponentInChildren<TrailRenderer>();

        if (!rigidBody)
        {
            rigidBody = gameObject.AddComponent<Rigidbody>();
            rigidBody.mass = 1;
            rigidBody.drag = 3;

            rigidBody.useGravity = true;

            rigidBody.isKinematic = false;
            rigidBody.constraints = RigidbodyConstraints.FreezeRotation;
        }
    }

    void Start()
    {
        InitObj();
        currJumpEnergy = jumpEnergyMax;
    }

    public void SwitchToPlaying()
    {
        rigidBody.useGravity = true;
        rigidBody.isKinematic = false;
        Cursor.lockState = CursorLockMode.Locked;
        isPlaying = true;
        rigidBody.constraints = RigidbodyConstraints.FreezeRotation;
    }

    public void SwitchOutOfPlaying()
    {
        rigidBody.useGravity = false;
        rigidBody.isKinematic = true;
        Cursor.lockState = CursorLockMode.None;
        rigidBody.constraints = RigidbodyConstraints.FreezeAll;
        isPlaying = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (rigidBody)
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.G))
            {
                if (isPlaying)
                {
                    SwitchOutOfPlaying();
                    transform.position = startingPos;
                    currJumpEnergy = jumpEnergyMax;
                }
                else SwitchToPlaying();
            }
            else if (Input.GetKeyDown(KeyCode.T))
                Cursor.lockState = CursorLockMode.Locked;

            if (Input.GetButtonDown("Sprint"))
            {
                CmdTurnOnTrailRenderer();
            }
            else if (Input.GetButtonUp("Sprint"))
            {
                CmdTurnOffTrailRenderer();
            }

            if (!isPlaying)
                return;

            if (!VRSettings.enabled)
            {
                float mouseX = Input.GetAxis("Mouse X");
                float mouseY = -Input.GetAxis("Mouse Y");

                rotY += mouseX * mouseSensitivity * Time.deltaTime;
                rotX += mouseY * mouseSensitivity * Time.deltaTime;

                rotX = Mathf.Clamp(rotX, -clampAngle, clampAngle);

                Quaternion localRotation = Quaternion.Euler(rotX, rotY, 0.0f);
                transform.rotation = localRotation;
            }

            if (Input.GetKey(KeyCode.Space) && currJumpEnergy > jumpCost * Time.deltaTime * 2f)
            {
                Jump(jumpFactor);
            }

        }

        if (isOnFloor && !Input.GetButton("Sprint"))
        {
            currJumpEnergy += Time.deltaTime * energyRegainRate * 5f;
        }
        else
        {
            currJumpEnergy += Time.deltaTime * energyRegainRate / 2f;
        }

        if (currJumpEnergy > jumpEnergyMax)
        {
            currJumpEnergy = jumpEnergyMax;
        }
    }

    private void FixedUpdate()
    {
        if (rigidBody && isPlaying)
        {
            Vector3 movement = transform.right * Input.GetAxis("Horizontal");

            movement = (movement + transform.forward * Input.GetAxis("Vertical")) * movementSpeed * 5f;

            if (Input.GetKey(KeyCode.Space))
            {
                movement *= slowFactor;
            }
            else if (Input.GetButton("Sprint") && isOnFloor && currJumpEnergy > jumpCost * Time.deltaTime * 2f)
            {
                movement *= sprintFactor;
                currJumpEnergy -= jumpCost * Time.deltaTime * 1.5f;
            }

            rigidBody.MovePosition(movement + transform.position);
        }
    }

    public void Jump(float jumpAmount)
    {
        rigidBody.AddForce(jumpAmount * Vector3.up);
        isOnFloor = false;

        currJumpEnergy -= jumpCost * Time.deltaTime;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if ((collision.gameObject.tag == "Platform") &&
            (transform.position.y > collision.transform.position.y + collision.transform.localScale.y / 2.1f))
        {
            isOnFloor = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Platform")
        {
            isOnFloor = false;
        }


    }

    [Command]
    private void CmdTurnOnTrailRenderer()
    {
        if (isLocalPlayer) return;

        if (!trailRenderer)
        {
            trailRenderer = GetComponentInChildren<TrailRenderer>();
        }

        if (trailRenderer)
            trailRenderer.time = 1f;
    }

    [Command]
    private void CmdTurnOffTrailRenderer()
    {
        if (isLocalPlayer) return;

        if (!trailRenderer)
        {
            trailRenderer = GetComponentInChildren<TrailRenderer>();
        }

        if (trailRenderer)
            trailRenderer.time = 0f;
    }

    [ClientRpc]
    public void RpcSlow()
    {
        isSlowed = true;
    }

    [ClientRpc]
    public void RpcUnSlow()
    {
        isSlowed = false;
    }
}