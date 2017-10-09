using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARCameraAvatar : MonoBehaviour {

    public GameObject rootPlayer;

#if UNITY_IOS
    private void Start()
    {
        GetComponent<Renderer>().enabled = false;
    }
#endif

}
