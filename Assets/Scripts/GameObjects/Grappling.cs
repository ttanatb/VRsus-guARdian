using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grappling : Launchable
{
    public float speed;
    public LayerMask collisionLayers;
    public float collisionCheckDistance;
    public float playerTravelTime;

    private bool attached = false;
    private float startTime;
    private Vector3 startPosition;

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