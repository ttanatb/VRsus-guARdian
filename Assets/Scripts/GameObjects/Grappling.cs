using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grappling : MonoBehaviour //: Launchable
{
    public float speed = 10f;
    public LayerMask collisionLayers;
    public float playerTravelSpeed = 3f;
    public float lifetime = 1f;

    public Grappling otherGrappling;
    public GameObject player;
    public string button;

    [SerializeField]
    private float timer;

    private Rigidbody rBody;

    private Movement playerMovement;
    private Rigidbody playerRBody;

    [SerializeField]
    private int state = 0;
    // 0 - inactive
    // 1 - shooting out
    // 2 - plyrTraveling
    // 3 - attached
    // 4 - retracting

    private float radii = 0;
    private float radiiSqr;
    private Vector3 force;

    private LineRenderer lineRenderer;
    public Transform playerAnchor;
    public Transform spawnPos;
    public Animator animController;

    public int State { get { return state; } }

    void Start()
    {
        timer = 0f;// Time.time;
        rBody = GetComponent<Rigidbody>();
        playerRBody = player.GetComponent<Rigidbody>();
        playerMovement = player.GetComponent<Movement>();
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.enabled = false;

        SphereCollider playerCol = player.GetComponentInChildren<SphereCollider>();
        radii = playerCol.radius;
        Transform playerColObj = playerCol.transform;
        do
        {
            radii *= playerColObj.localScale.x;
            playerColObj = playerColObj.parent;
        }
        while (playerColObj != null);

        radii += GetComponent<SphereCollider>().radius + transform.localScale.x;
        radiiSqr = radii * radii;
    }

    // Update is called once per frame
    private void Update()
    {
        if (state == 0)     //check input if it's inactive
        {
            CheckShoot();
        }
        else
        {
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, playerAnchor.position);

            if (state == 1)    //updates lifetime if its shooting out
            {
                if (timer > lifetime || !Input.GetButton(button))
                    StartRetraction();

                timer += Time.deltaTime;
            }
            else if (state == 3)
            {
                if (!Input.GetButton(button))
                    Detach();
            }
        }
    }

    private void FixedUpdate()
    {
        if (state == 2)     //player is traveling, move player accordingly
        {
            if (!Input.GetButton(button))
            {
                animController.SetBool("isFlying", false);
                LetGo();
            }
            else
            {
                MovePlayer();
            }
        }
        else if (state == 4)    //grappling hook is retracting
        {
            RetractHookshot();
        }
    }

    private void MovePlayer()   //moves player
    {
        force = (transform.position - player.transform.position).normalized * playerTravelSpeed;
        playerRBody.AddForce(force);
        if ((transform.position - player.transform.position).sqrMagnitude < 0.01f)
        {
            state = 3;
            playerRBody.velocity = Vector3.zero;
            animController.SetBool("isClinging", true);
            animController.SetBool("isFlying", false);
        }
    }

    private void RetractHookshot()  //the hookshot travels back to the player
    {
        transform.position = Vector3.Lerp(transform.position, playerAnchor.position, 0.2f);
        if ((playerAnchor.position - transform.position).sqrMagnitude < 0.02f || timer > lifetime)
        {
            state = 0;
            transform.position = Vector3.one * 1000f;
            lineRenderer.enabled = false;
        }
    }

    private void StartRetraction()
    {
        //Debug.Log("Grappling Hook is retracting");
        playerMovement.EnableMovement();
        rBody.velocity = -rBody.velocity;
        timer = 0f;// Time.time;
        state = 4;
    }


    private void LetGo()    //let go of the hookshot while traveling
    {
        //Debug.Log("Player let go of grappling hook while traveling");
        rBody.isKinematic = false;
        playerRBody.useGravity = true;
        StartRetraction();
    }

    private void Detach()
    {
        //Debug.Log("Player detaching himself from the wall");
        playerRBody.useGravity = true;
        animController.SetBool("isClinging", false);
        if (Input.GetButton("Jump"))
        {
            playerRBody.AddForce(Vector3.up * 5f, ForceMode.VelocityChange);
        }
        StartRetraction();
    }

    private void CheckShoot()
    {
        if (Input.GetButtonDown(button) && !playerMovement.IsSlowed)
        {
            //Debug.Log("Player shot the grappling hook");
            if (otherGrappling.state == 1) otherGrappling.StartRetraction();
            else if (otherGrappling.state == 2) otherGrappling.LetGo();

            transform.position = playerAnchor.position;
            transform.forward = playerAnchor.forward;

            rBody.velocity = transform.forward * speed;
            rBody.isKinematic = false;
            state = 1;

            lineRenderer.enabled = true;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (state != 1) return;

        if (collisionLayers == (collisionLayers | (1 << collision.collider.gameObject.layer)))
        {
            //Debug.Log("Grappling hook attached itself to the wall");
            if (otherGrappling.state == 3)
            {
                otherGrappling.LetGo();
                animController.SetBool("isClinging", false);
            }

            state = 2;
            playerMovement.isOnFloor = false;
            playerMovement.DisableMovement();
            rBody.isKinematic = true;

            playerRBody.useGravity = false;
            timer = 0f;// Time.time;
            animController.SetBool("isFlying", true);
        }
        else
        {
            StartRetraction();
        }
    }
}