using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Decoy : Launchable
{
    public float lifetime;
    public float speed;
    public LayerMask collisionLayers;
    public float collisionCheckDistance;

    private bool moving = false;
	
	// Update is called once per frame
	void FixedUpdate ()
    {
		if (moving)
        {
            RaycastHit[] hits = Physics.RaycastAll(transform.position + transform.up, -transform.up);
            transform.up = hits[0].normal;

            transform.position += (transform.forward * speed);
        }
	}

    void OnCollisionEnter(Collision collision)
    {
        if (collisionLayers == (collisionLayers | (1 << collision.collider.gameObject.layer)))
        {
            moving = true;
            Destroy(gameObject, lifetime);
        }
    }
}
