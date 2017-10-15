using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAvatar : MonoBehaviour {

    public Combat rootPlayer;

    private void Update()
    {
#if UNITY_IOS
        Debug.Log("Box collider: " + GetComponent<BoxCollider>());

#endif
    }
}
