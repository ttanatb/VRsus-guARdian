using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectLauncher : MonoBehaviour {

    public GameObject player;
    public GameObject launchObject;
    public float force;
	
	// Update is called once per frame
	void Update () {
		if (Input.GetButtonDown("RightTrigger"))
        {
            GameObject temp = Instantiate(launchObject, transform.position, transform.rotation);
            temp.GetComponent<Rigidbody>().AddForce(transform.forward * force, ForceMode.Impulse);
            temp.GetComponent<Launchable>().Player = player;
        }
	}
}
