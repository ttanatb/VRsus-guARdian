using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAvatar : MonoBehaviour {

    public Combat rootPlayer;

#if UNITY_IOS
    private void Start()
    {
        GetComponent<Renderer>().enabled = false;
    }
#endif

}
