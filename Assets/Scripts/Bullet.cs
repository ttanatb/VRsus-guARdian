using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Bullet : NetworkBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (!isServer)
            return;

        //gets Combat from the colliding gameObject
        Combat combat = collision.gameObject.GetComponent<Combat>();

        //if it's the AR object try the gameobject from root
        if (!combat)
        {
            //get root
            ARCameraAvatar arChar = collision.gameObject.GetComponent<ARCameraAvatar>();
            if (arChar)
                combat = arChar.rootPlayer.GetComponent<Combat>();
        }

        //if there's combat
        if (combat)
        {
            combat.TakeDamage();
            Destroy(gameObject);
        }
    }
}
