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
    public GameObject ARPlayer;
    public GameObject VRPlayer;

    private AudioListener[] listeners;
    private Camera[] cameras;

    private PlayerType playerType;

    private void Awake()
    {
        listeners = GetComponentsInChildren<AudioListener>();
    }

    // Use this for initialization
    void Start()
    {
        if (!ARPlayer)
        {
            ARPlayer = transform.GetChild(0).gameObject;
        }

        if (!VRPlayer)
        {
            VRPlayer = transform.GetChild(1).gameObject;
        }



#if UNITY_IOS
         playerType = PlayerType.AR;
#else
        playerType = PlayerType.VR;
#endif

        if (!isLocalPlayer)
        {
            foreach(AudioListener listener in listeners)
            {
                listener.enabled = false;
            }

            foreach(Camera cam in cameras)
            {
                cam.enabled = false;
            }
        }

        if (playerType == PlayerType.AR)
        {
            foreach(Camera cam in VRPlayer.GetComponentsInChildren<Camera>())
            {
                cam.enabled = false;
                cam.GetComponent<AudioListener>().enabled = false;
            }

            foreach(GameObject obj in GameObject.FindGameObjectsWithTag("VROnly"))
            {
                obj.SetActive(false);
            }

#if UNITY_IOS
            UnityARCameraManager.Instance.SetCamera(Camera.main);
#endif
        }

        else
        {
            foreach (Camera cam in ARPlayer.GetComponentsInChildren<Camera>())
            {
                cam.enabled = false;
                cam.GetComponent<AudioListener>().enabled = false;
            }

            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("AROnly"))
            {
                obj.SetActive(false);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
