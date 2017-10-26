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
    public Object[] ObjsForARAvatarThatVRPlayerCanSee;
    public Object[] ObjsForVRAvatarThatARPlayerCanSee;

    public GameObject ARAvatar;
    public GameObject VRAvatar;

    public GameObject ARCamera;
    public GameObject VRCamera;

    [SerializeField]
    private PlayerType playerType;

    [SerializeField]
    private bool isThisALocalPlayer;

    [SerializeField]
    private bool isThisAServer;

    public PlayerType PlayerType { get { return playerType; } }

    private void Update()
    {
        isThisALocalPlayer = isLocalPlayer;
        isThisAServer = isServer;
    }

    // Use this for initialization
    void Start()
    {
        if (isServer && isLocalPlayer)
        {
            GetComponent<GameManager>().enabled = true;
        } 


        //set up if the Player should be VR or AR
        if (isLocalPlayer)
        {
#if UNITY_IOS
            playerType = PlayerType.AR;
#else
            playerType = PlayerType.VR;
#endif
        }
        else
        {
#if UNITY_IOS
            playerType = PlayerType.VR;
#else
            playerType = PlayerType.AR;
#endif
        }

        if (playerType == PlayerType.AR)
        {
            VRAvatar.GetComponent<Collider>().enabled = false;
        }

        else
        {
            ARAvatar.GetComponent<Collider>().enabled = false;

        }

        //Enable objects or scripts of the other player
        if (!isLocalPlayer)
        {
            if (playerType == PlayerType.AR)
            {
                ARAvatar.GetComponent<Renderer>().enabled = true;

                foreach (Object o in ObjsForARAvatarThatVRPlayerCanSee)
                {
                    if (o is GameObject)
                        ((GameObject)o).SetActive(true);
                    else if (o is MonoBehaviour)
                        ((MonoBehaviour)o).enabled = true;
                }

            }
            else
            {
                VRAvatar.GetComponent<Renderer>().enabled = true;

                foreach (Object o in ObjsForVRAvatarThatARPlayerCanSee)
                {
                    if (o is GameObject)
                        ((GameObject)o).SetActive(true);
                    else if (o is MonoBehaviour)
                        ((MonoBehaviour)o).enabled = true;
                }
            }
        }

        //enable camera and audio listener for player
        else
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

        }

        if (playerType == PlayerType.AR)
        {
            if (GetComponent<PlaneManager>())
                GetComponent<PlaneManager>().enabled = true;
            if (GetComponent<Movement>())
                Destroy(GetComponent<Movement>());
            if (GetComponent<BlockManager>())
                Destroy(GetComponent<BlockManager>());
        }
        else
        {
            if (GetComponent<PlaneManager>())
                Destroy(GetComponent<PlaneManager>());
            if (GetComponent<Movement>())
                GetComponent<Movement>().enabled = true;
            if (GetComponent<BlockManager>())
                GetComponent<BlockManager>().enabled = true;
        }

        if (GetComponent<Combat>())
            GetComponent<Combat>().enabled = true;
    }
}