﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public enum PlayerType
{
    AR,
    VR
}

public class Player : NetworkBehaviour
{
    //public GameObject ARPlayer;
    //public GameObject VRPlayer;

    public GameObject ARAvatar;
    public GameObject VRAvatar;

    public GameObject ARCamera;
    public GameObject VRCamera;

    [SerializeField]
    private PlayerType playerType;

    public PlayerType PlayerType { get { return playerType; } }

    private void Awake()
    {
        if (!isLocalPlayer)
        {
#if UNITY_IOS
            playerType = PlayerType.VR;
#else
            playerType = PlayerType.AR;
#endif
        }
    }

    public override void OnStartLocalPlayer()
    {
#if UNITY_IOS
         playerType = PlayerType.AR;
#else
        playerType = PlayerType.VR;
#endif

        if (playerType == PlayerType.AR)
        {
            ARCamera.GetComponent<Camera>().enabled = true;
            ARCamera.GetComponent<AudioListener>().enabled = true;

#if UNITY_IOS
            UnityARCameraManager.Instance.SetCamera(Camera.main);
#endif
        }

        else
        {
            VRCamera.GetComponent<Camera>().enabled = true;
            VRCamera.GetComponent<AudioListener>().enabled = true;
        }


        base.OnStartLocalPlayer();
    }

    // Use this for initialization
    void Start()
    {
        if (!isLocalPlayer)
        {
            if (playerType == PlayerType.AR)
            {
                ARAvatar.SetActive(true);
            }
            else
            {
                VRAvatar.SetActive(true);
            }
        }

    }
}