using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Initilizes what the player should be, and what scripts should be enabled/destroyed
/// 
/// Author: Tanat Boozayaangool
/// </summary>
public class PlayerInitializer : NetworkBehaviour
{
    #region Fields
    [Tooltip("Renderers attached to the AR Player that only the VR player should be able to see")]
    public Object[] ARRenderersOnlyForVR;

    [Tooltip("Renderers attached to the VR Player that only the AR player should be able to see")]
    public Object[] VRRenderersOnlyForAR;

    public GameObject ARAvatar;
    public GameObject VRAvatar;

    public GameObject ARCamera;
    public GameObject VRCamera;

    public Collider VRCollider;

    [SerializeField]
    [SyncVar]
    private int playerType;

    public PlayerType PlayerType
    {
        get { return (PlayerType)playerType; }
        set { playerType = (int)value; }
    }
    #endregion

    #region Init Logic
    void Start()
    {
        if (isLocalPlayer)
        {
            if (isServer)
                PlayerType = PlayerType.AR;
            else PlayerType = PlayerType.VR;
        
            EnableCameraAndAudioListener();
        }
        
        else
        {
            if (isServer)
                PlayerType = PlayerType.VR;
            else PlayerType = PlayerType.AR;
        
            EnablePlayerRenderers();
        }

        //Enabling stuff for both local player and remote players
        if (PlayerType == PlayerType.AR)
        {
            GetComponent<NetworkTransformChild>().target = ARCamera.transform;
            ARAvatar.GetComponent<Collider>().enabled = true;
        }
        else
        {
            gameObject.layer = LayerMask.NameToLayer("VRPlayer");

            GetComponent<NetworkTransformChild>().target = VRAvatar.transform;
            VRCollider.enabled = true;
        }

        InitComponents();
    }
    #endregion

    #region Helper Functions

    /// <summary>
    /// Enable renderers for the appropriate player type
    /// </summary>
    public void EnablePlayerRenderers()
    {
        if (PlayerType == PlayerType.AR)
        {
            Renderer[] ARrenderers = ARAvatar.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in ARrenderers)
                r.enabled = true;

            foreach (Object o in ARRenderersOnlyForVR)
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
            foreach (Object o in VRRenderersOnlyForAR)
            {
                if (o is GameObject)
                    ((GameObject)o).SetActive(true);
                else if (o is MonoBehaviour)
                    ((MonoBehaviour)o).enabled = true;
            }
        }
    }

    /// <summary>
    /// Enable camera/audio listeners of local player
    /// </summary>
    private void EnableCameraAndAudioListener()
    {
        if (PlayerType == PlayerType.VR)
        {
            VRCamera.GetComponent<Camera>().enabled = true;
            VRCamera.GetComponent<AudioListener>().enabled = true;
        }
        else
        {
            ARCamera.GetComponent<Camera>().enabled = true;
            ARCamera.GetComponent<AudioListener>().enabled = true;

#if UNITY_IOS //if on AR-enabled device
            // Initialize AR stuff with camera
            UnityARCameraManager.Instance.SetCamera(Camera.main);
#else
            // Turns camera into a fps camera
            Destroy(ARCamera.GetComponent<UnityARCameraNearFar>());
            Camera arCam = ARCamera.GetComponent<Camera>();
            Camera vrCam = VRCamera.GetComponent<Camera>();

            arCam.clearFlags = vrCam.clearFlags;
            arCam.nearClipPlane = vrCam.nearClipPlane;
            arCam.farClipPlane = vrCam.farClipPlane;
            arCam.allowHDR = vrCam.allowHDR;
            arCam.depth = vrCam.depth;
#endif
        }
    }

    /// <summary>
    /// Initializes all other player components
    /// </summary>
    private void InitComponents()
    {
        if (PlayerType == PlayerType.AR)
        {
            EnablePlayerComponent<ARPlaneManager>();
            EnablePlayerComponent<ARSetUp>();
            EnablePlayerComponent<ARCombat>();
#if UNITY_IOS
            DestroyPlayerComponent<Movement>();
#else
            EnablePlayerComponent<Movement>();
#endif
            DestroyPlayerComponent<VRTransition>();
            DestroyPlayerComponent<VRCombat>();
        }
        else
        {
            DestroyPlayerComponent<ARPlaneManager>();
            DestroyPlayerComponent<ARSetUp>();
            DestroyPlayerComponent<ARCombat>();
            EnablePlayerComponent<Movement>();
            EnablePlayerComponent<VRTransition>();
            EnablePlayerComponent<VRCombat>();
        }
    }

    private void DestroyPlayerComponent<T>()
    {
        PlayerComponent component = GetComponent<T>() as PlayerComponent;

        if (component)
        {
            component.InitMemberFields(PlayerType, this);
            component.Destroy();
        }
    }

    private void EnablePlayerComponent<T>()
    {
        PlayerComponent component = GetComponent<T>() as PlayerComponent;

        if (component)
        {
            component.InitMemberFields(PlayerType, this);
            component.enabled = true;
        }
    }

#endregion
}