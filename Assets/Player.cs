using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public GameObject iOSPlayer;
    public GameObject VRPlayer;

    // Use this for initialization
    void Start()
    {
        if (!iOSPlayer)
        {
            iOSPlayer = transform.GetChild(0).gameObject;
        }

        if (!VRPlayer)
        {
            VRPlayer = transform.GetChild(1).gameObject;
        }


#if UNITY_IOS
        VRPlayer.SetActive(false);
        iOSPlayer.SetActive(true);
#else
        VRPlayer.SetActive(true);
        iOSPlayer.SetActive(false);
#endif
    }

    // Update is called once per frame
    void Update()
    {

    }
}
