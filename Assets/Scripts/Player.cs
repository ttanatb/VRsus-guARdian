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

    //[SyncVar]
    //private int relicCount = 0;

    private void Update()
    {
#if UNITY_IOS
    //Debug.Log("VR ColliderPos: " + VRAvatar.transform.position);
#endif
    }

    // Use this for initialization
    void Start()
    {
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

        if (isLocalPlayer)
        {
            if (playerType == PlayerType.VR)
            {
                //VRAvatar.GetComponent<Collider>().enabled = true;
            }
            else
            {
                ARAvatar.GetComponent<Collider>().enabled = true;
            }
        }
        else
        {
            if (playerType == PlayerType.AR)
            {
                //VRAvatar.GetComponent<Collider>().enabled = true;
            }
            else
            {
                ARAvatar.GetComponent<Collider>().enabled = true;
            }
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
                /*
                VRAvatar.GetComponent<Renderer>().enabled = true;

                foreach (Object o in ObjsForVRAvatarThatARPlayerCanSee)
                {
                    if (o is GameObject)
                        ((GameObject)o).SetActive(true);
                    else if (o is MonoBehaviour)
                        ((MonoBehaviour)o).enabled = true;
                }
                */
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

            if (GetComponent<GameManager>())
                GetComponent<GameManager>().enabled = true;
        }
        else
        {
            if (GetComponent<PlaneManager>())
                Destroy(GetComponent<PlaneManager>());
            if (GetComponent<Movement>())
                GetComponent<Movement>().enabled = true;
            if (GetComponent<BlockManager>())
                GetComponent<BlockManager>().enabled = true;


            if (GetComponent<GameManager>())
                Destroy(GetComponent<GameManager>());
        }

        if (GetComponent<Combat>())
            GetComponent<Combat>().enabled = true;
    }

    public void EnableVRPlayerRenderers()
    {
        VRAvatar.GetComponent<Renderer>().enabled = true;
        VRAvatar.GetComponent<Collider>().enabled = true;

        foreach (Object o in ObjsForVRAvatarThatARPlayerCanSee)
        {
            if (o is GameObject)
                ((GameObject)o).SetActive(true);
            else if (o is MonoBehaviour)
                ((MonoBehaviour)o).enabled = true;
        }
    }

    public void GainRelic()
    {
        //srelicCount += 1;
    }
}