using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class Player : NetworkBehaviour
{
    public GameObject iOSPlayer;
    public GameObject VRPlayer;

    private AudioListener[] listeners;

    private void Awake()
    {
        listeners = GetComponentsInChildren<AudioListener>();
    }

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

        if (!isClient)
        {
            foreach(AudioListener listener in listeners)
            {
                listener.enabled = false;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
