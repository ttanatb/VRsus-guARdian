using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.VR;

//FPS camera movement adapted from: http://wiki.unity3d.com/index.php/SmoothMouseLook
public class Movement : PlayerComponent
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

    [SerializeField]
    private bool isPlaying = true;

    public float mouseSensitivity = 100.0f;
    public float clampAngle = 80.0f;

    private float rotY = 0.0f; // rotation around the up/y axis
    private float rotX = 0.0f; // rotation around the right/x axis

    public uint frameCounter = 20;
    Vector3 startingPos;

    public float slowFactor = 0.3f;
    private float slowTimer;
    private const float MAX_SLOW_TIME = 2f;
    TrailRenderer trailRenderer;

    public bool isOnFloor = false;

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
        if (!trailRenderer && playerType == PlayerType.VR)
            trailRenderer = GetComponentInChildren<TrailRenderer>();

        if (!rigidBody)
        {
            rigidBody = gameObject.AddComponent<Rigidbody>();
            rigidBody.mass = 1;
            rigidBody.drag = 5;

            if (playerType == PlayerType.AR)
                rigidBody.useGravity = false;
            else rigidBody.useGravity = true;

            rigidBody.isKinematic = false;
            rigidBody.constraints = RigidbodyConstraints.FreezeRotation;
        }
    }

    void Start()
    {
        isPlaying = true;
        InitObj();
        currJumpEnergy = jumpEnergyMax;
        //CmdTurnOffTrailRenderer();
    }

    public override void OnStartLocalPlayer()
    {
        if (playerType == PlayerType.VR)
        {
            CanvasManager.Instance.InitJumpEnergyBar(this);
        }
    }

    public void SwitchToPlaying()
    {
        if (!isLocalPlayer) return;
        if (playerType == PlayerType.AR)
            rigidBody.useGravity = false;
        else rigidBody.useGravity = true;
        rigidBody.isKinematic = false;
        Cursor.lockState = CursorLockMode.Locked;
        isPlaying = true;
        rigidBody.constraints = RigidbodyConstraints.FreezeRotation;
    }

    public void SwitchOutOfPlaying()
    {
        if (!isLocalPlayer) return;
        rigidBody.useGravity = false;
        rigidBody.isKinematic = true;
        Cursor.lockState = CursorLockMode.None;
        rigidBody.constraints = RigidbodyConstraints.FreezeAll;
        isPlaying = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer) return;
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.D)) Cursor.lockState = CursorLockMode.None;
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

            if (!UnityEngine.XR.XRSettings.enabled)
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

                Quaternion localRotation = Quaternion.Euler(rotX, rotY, 0.0f);
                transform.rotation = localRotation;
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
        slowTimer -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (rigidBody && isPlaying && isLocalPlayer)
        {
            if (!UnityEngine.XR.XRSettings.enabled)
            {
                Vector3 movement = transform.right * Input.GetAxis("Horizontal");

                if (playerType == PlayerType.AR)
                {
                    movement = (movement + transform.forward * Input.GetAxis("Vertical")) * movementSpeed;
                }
                else
                {
                    movement = (movement + Vector3.ProjectOnPlane(transform.forward, Vector3.up) * Input.GetAxis("Vertical")) * movementSpeed;
                }
                if (playerType == PlayerType.AR)
                {
                    movement *= 3.5f;
                }

                if (Input.GetButton("Sprint") && isOnFloor && currJumpEnergy > jumpCost * Time.deltaTime * 2f)
                {
                    movement *= sprintFactor;
                    currJumpEnergy -= jumpCost * Time.deltaTime * 1.5f;
                }
                if (Input.GetKey(KeyCode.Space) && currJumpEnergy > jumpCost * Time.deltaTime * 2f)
                {
                    movement += Jump(jumpFactor);
                }

                if (slowTimer > 0f)
                    movement *= Mathf.Lerp(1.0f, slowFactor, slowTimer / MAX_SLOW_TIME);

                rigidBody.AddForce(movement, ForceMode.Acceleration);
            }
            else
            {
                Vector3 movement = Vector3.zero;

                if (Input.GetAxis("VRLeftHorizontal") != 0 || Input.GetAxis("VRLeftVertical") != 0)
                {
                    movement = transform.Find("[CameraRig]").Find("Controller (left)").right * Input.GetAxis("VRLeftHorizontal");

                    movement = (movement + transform.Find("[CameraRig]").Find("Controller (left)").forward * Input.GetAxis("VRLeftVertical")) * movementSpeed;
                }
                else if (Input.GetAxis("VRRightHorizontal") != 0 || Input.GetAxis("VRRightVertical") != 0)
                {
                    movement = transform.Find("[CameraRig]").Find("Controller (right)").right * Input.GetAxis("VRRightHorizontal");

                    movement = (movement + transform.Find("[CameraRig]").Find("Controller (right)").forward * Input.GetAxis("VRRightVertical")) * movementSpeed;
                }

                if (slowTimer > 0f)
                    movement *= Mathf.Lerp(1.0f, slowFactor, slowTimer / MAX_SLOW_TIME);

                rigidBody.MovePosition(movement + transform.position);
            }
        }
    }

    public Vector3 Jump(float jumpAmount)
    {
        if (playerType == PlayerType.AR) return Vector3.zero;

        isOnFloor = false;
        currJumpEnergy -= jumpCost * Time.deltaTime;
        return jumpAmount * Vector3.up;
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
        slowTimer = MAX_SLOW_TIME;
    }
}