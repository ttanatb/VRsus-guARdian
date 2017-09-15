using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
		Combat combat = collision.gameObject.GetComponent<Combat> ();
		if (!combat) {
			ARAvatar arChar = collision.gameObject.GetComponent<ARAvatar> ();
			if (arChar)
				combat = arChar.rootPlayer.GetComponent<Combat> ();
		}

		if (combat)
        {
            combat.TakeDamage();
            Destroy(gameObject);
        }
    }
}
