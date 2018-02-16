using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

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

    private bool isPlaying = false;

    public float sensitivityX = 15f;
    public float sensitivityY = 15f;

    public float minX = -360f;
    public float maxX = 360f;

    public float minY = -60f;
    public float maxY = 60f;

    private float rotX = 0f;
    private float rotY = 0f;

    private List<float> rotListX = new List<float>();
    float avgRotX = 0f;

    private List<float> rotListY = new List<float>();
    float avgRotY = 0f;

    public uint frameCounter = 20;
    Quaternion startingRot;
    Vector3 startingPos;

    private float slowTimer;
    private const float MAX_SLOW_TIME = 2f;
    public float slowFactor = 0.7f;
    TrailRenderer trailRenderer;

    public bool isOnFloor = false;

    public float CurrJumpEnergy { get { return currJumpEnergy; } }

    // Use this for initialization
    void Awake()
    {
        startingRot = transform.rotation;
        startingPos = transform.position;
    }

    protected override void InitObj()
    {
        if (!trailRenderer && playerType == PlayerType.VR)
            trailRenderer = GetComponentInChildren<TrailRenderer>();

        if (!rigidBody)
        {
            rigidBody = gameObject.AddComponent<Rigidbody>();
            rigidBody.mass = 1;
            rigidBody.drag = 3;

            if (playerType == PlayerType.AR)
                rigidBody.useGravity = false;
            else rigidBody.useGravity = true;

            rigidBody.isKinematic = false;
            rigidBody.constraints = RigidbodyConstraints.FreezeRotation;
        }
    }

    void Start()
    {
        InitObj();
        if (isLocalPlayer)
        {
            CmdTurnOffTrailRenderer();
            currJumpEnergy = jumpEnergyMax;
            if (playerType == PlayerType.VR)
                CanvasManager.Instance.InitJumpEnergyBar(this);
            else SwitchToPlaying();
        }
    }

    public void SwitchToPlaying()
    {
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
        rigidBody.useGravity = false;
        rigidBody.isKinematic = true;
        Cursor.lockState = CursorLockMode.None;
        rigidBody.constraints = RigidbodyConstraints.FreezeAll;
        isPlaying = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.D)) Cursor.lockState = CursorLockMode.None;
        if (rigidBody)
        {
            if (!isLocalPlayer) return;

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

            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                CmdTurnOnTrailRenderer();
            }
            else if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                CmdTurnOffTrailRenderer();
            }

            if (!isPlaying)
                return;

            avgRotX = 0f;
            avgRotY = 0f;

            rotY += Input.GetAxis("Mouse Y") * sensitivityY * Time.deltaTime;
            rotX += Input.GetAxis("Mouse X") * sensitivityX * Time.deltaTime;

            rotListY.Add(rotY);
            rotListX.Add(rotX);

            if (rotListX.Count > frameCounter)
            {
                rotListX.RemoveAt(0);
            }

            if (rotListY.Count > frameCounter)
            {
                rotListY.RemoveAt(0);
            }

            for (int i = 0; i < rotListX.Count; i++)
            {
                avgRotX += rotListX[i];
                avgRotY += rotListY[i];
            }

            avgRotX /= rotListX.Count;
            avgRotY /= rotListY.Count;

            avgRotY = ClampAngle(avgRotY, minY, maxY);
            avgRotX = ClampAngle(avgRotX, minX, maxX);

            Quaternion yRot = Quaternion.AngleAxis(avgRotY, Vector3.left);
            Quaternion xRot = Quaternion.AngleAxis(avgRotX, Vector3.up);

            transform.rotation = startingRot * xRot * yRot;



        }

        if (isOnFloor && !Input.GetKey(KeyCode.LeftShift))
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
        if (rigidBody && isPlaying)
        {
            Vector3 movement = transform.right * Input.GetAxis("Horizontal");

            if (playerType == PlayerType.AR)
                movement = (movement + transform.forward * Input.GetAxis("Vertical")) * movementSpeed * 5f;
            else movement = (movement + (Vector3.ProjectOnPlane(transform.forward, Vector3.down) * Input.GetAxis("Vertical"))) * movementSpeed;

            if (Input.GetKey(KeyCode.Space))
            {
                //movement *= slowFactor;
            }
            else if (Input.GetKey(KeyCode.LeftShift) && isOnFloor && currJumpEnergy > jumpCost * Time.deltaTime * 2f)
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

            rigidBody.MovePosition(movement + transform.position);
        }
    }

    public Vector3 Jump(float jumpAmount)
    {
        if (playerType == PlayerType.AR) return Vector3.zero;

        isOnFloor = false;
        currJumpEnergy -= jumpCost * Time.deltaTime;
        return jumpAmount * Vector3.up;
    }

    float ClampAngle(float angle, float min, float max)
    {
        angle %= 360;
        if ((angle >= -360f) && (angle <= 360f))
        {
            if (angle < -360f)
            {
                angle += 360f;
            }
            if (angle > 360f)
            {
                angle -= 360f;
            }
        }

        return Mathf.Clamp(angle, min, max);
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