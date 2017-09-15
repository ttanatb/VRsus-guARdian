using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        Combat combat;
        if ((combat = collision.gameObject.GetComponent<Combat>()) || (combat = collision.gameObject.GetComponent<ARAvatar>().rootPlayer.GetComponent<Combat>()))
        {
            combat.TakeDamage();
            Destroy(gameObject);
        }
    }
}
