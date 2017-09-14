using System.Collections;
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

    private AudioListener[] listeners;
    private Camera[] cameras;

    private PlayerType playerType;

    public PlayerType PlayerType { get { return playerType; } }

    private void Awake()
    {
        listeners = GetComponentsInChildren<AudioListener>();
        cameras = GetComponentsInChildren<Camera>();

#if UNITY_IOS
         playerType = PlayerType.AR;
#else
        playerType = PlayerType.VR;
#endif
    }

    public override void OnStartLocalPlayer()
    {
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
                VRAvatar.SetActive(true);
            }

            else
            {
                ARAvatar.SetActive(true);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
