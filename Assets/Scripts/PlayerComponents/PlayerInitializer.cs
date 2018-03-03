using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Initilizes what the player should be, and what scripts should be enabled/destroyed
/// 
/// Author: Tanat Boozayaangool
/// </summary>
/// 

public class PlayerInitializer : NetworkBehaviour
{
    #region Fields
    public PlayerType playerType;
    public Renderer[] renderersToDisbale;
    public Renderer[] renderersToEnable;
    public Camera cameraToEnable;

    public Camera[] camerasToDisable;

    public GameObject avatar;

    public PlayerType PlayerType
    {
        get { return playerType; }
        set { playerType = value; }
    }
    #endregion

    #region Init Logic
    public override void OnStartLocalPlayer()
    {
        //Disables any renderers it has to
        int count = renderersToDisbale.Length;
        for (int i = 0; i < count; i++)
            renderersToDisbale[i].enabled = false;


        count = renderersToEnable.Length;
        for (int i = 0; i < count; i++)
            renderersToEnable[i].enabled = true;

        //Enables its camera and audio listenrs
        if (cameraToEnable)
        {
            cameraToEnable.enabled = true;
            cameraToEnable.GetComponent<AudioListener>().enabled = true;
            cameraToEnable.gameObject.tag = "MainCamera";
        }



        if (playerType != PlayerType.VR)
        {
            UnityEngine.XR.XRSettings.enabled = false;
        }

        switch (playerType)
        {
            case PlayerType.AR:
#if UNITY_IOS //if on AR-enabled device
                // Initialize AR stuff with camera
                UnityARCameraManager.Instance.SetCamera(Camera.main);

                Movement m = GetComponent<Movement>();
                if (m) Destroy(m);
#else
                // Turns camera into a fps camera
                Destroy(cameraToEnable.GetComponent<UnityARCameraNearFar>());

                cameraToEnable.clearFlags = CameraClearFlags.Skybox;
                cameraToEnable.nearClipPlane = 0.01f;
                cameraToEnable.farClipPlane = 75f;
                cameraToEnable.allowHDR = true;
                cameraToEnable.depth = 0;
#endif
                break;
            case PlayerType.VR:
                break;
            case PlayerType.PC:
                break;
        }
    }

    private void Start()
    {
        PlayerComponent[] components = GetComponents<PlayerComponent>();
        //Debug.Log(components.Length);
        for (int i = 0; i < components.Length; i++)
        {
            components[i].InitMemberFields(playerType, this);
            components[i].enabled = true;
        }

        if (!isLocalPlayer)
        {
            for (int i = 0; i < camerasToDisable.Length; i++)
            {
                camerasToDisable[i].enabled = false;
            }
        }
    }
    #endregion
}