using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmokeBomb : Launchable
{
    public LayerMask collisionLayers;

    void Start()
    {
        GetComponent<ParticleSystem>().Stop();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collisionLayers == (collisionLayers | (1 << collision.collider.gameObject.layer)))
        {
            GetComponent<ParticleSystem>().Play();
            Destroy(gameObject, 5.0f);
        }
    }
}
