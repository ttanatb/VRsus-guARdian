using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseColliderScript : MonoBehaviour
{
    public Transform cameraEye;
	
	// Update is called once per frame
	void Update ()
    {
        transform.localPosition = new Vector3(cameraEye.localPosition.x, 0, cameraEye.localPosition.z);
	}
}
