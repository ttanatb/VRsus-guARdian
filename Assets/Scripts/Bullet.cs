using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player" || collision.gameObject.tag == "ARPlayer")
        {
			Debug.Log ("Blargh");

            Combat combat = collision.gameObject.GetComponent<Combat>();
            if (!combat)
                combat = collision.gameObject.GetComponent<ARAvatar>().rootPlayer.GetComponent<Combat>();

            combat.TakeDamage();
            Destroy(gameObject);
        }
    }
}
