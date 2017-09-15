using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
	public string owner = "";

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player" || collision.gameObject.tag == "ARPlayer")
        {
			if ((collision.gameObject.tag == "Player" && owner == "Player") 
				|| (collision.gameObject.tag == "ARPlayer" && owner == "ARPlayer"))
				Destroy (gameObject);
			
            Combat combat = collision.gameObject.GetComponent<Combat>();
            if (!combat)
                combat = collision.gameObject.GetComponent<ARAvatar>().rootPlayer.GetComponent<Combat>();

            combat.TakeDamage();
            Destroy(gameObject);
        }
    }
}
