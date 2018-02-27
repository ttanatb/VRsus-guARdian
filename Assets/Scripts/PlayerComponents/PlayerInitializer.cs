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
    public Camera cameraToEnable;

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

        //Enables its camera and audio listenrs
        cameraToEnable.enabled = true;
        cameraToEnable.GetComponent<AudioListener>().enabled = true;
        cameraToEnable.gameObject.tag = "MainCamera";

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
                cameraToEnable.farClipPlane = 1000f;
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
    }
    #endregion
}