using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grappling : Launchable
{
    public float speed;
    public LayerMask collisionLayers;
    public float collisionCheckDistance;
    public float playerTravelTime;
    public float lifetime;

    private bool attached = false;
    private float startTime;
    private Vector3 startPosition;

    void Start()
    {
        startTime = Time.time;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (attached)
        {
            float percent = (Time.time - startTime) / playerTravelTime;
            if (percent > 1.0f) percent = 1.0f;

            Player.transform.position = Vector3.Slerp(startPosition, transform.position, percent);

            if (Player.transform.position == transform.position)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            transform.position += transform.forward * speed;
            if (Time.time - startTime > lifetime)
            {
                Destroy(gameObject);
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collisionLayers == (collisionLayers | (1 << collision.collider.gameObject.layer)))
        {
            GetComponent<Rigidbody>().isKinematic = true;

            attached = true;

            startTime = Time.time;
            startPosition = Player.transform.position;
        }
    }
}