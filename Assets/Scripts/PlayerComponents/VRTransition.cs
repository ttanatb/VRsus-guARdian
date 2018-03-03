using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Manages the transition between different cameras for the VR player
/// 
/// To-do's: A function to move player to training area
/// </summary>
public class VRTransition : PlayerComponent
{
    #region Fields
    //cameras
    private Camera fpsCamera;

    //the speed of fading
    [Range(0f, 1f)]
    public float fadeSpeed;

    //wait time between fade in/out
    public float waitTime = 1.2f;

    //player renderer
    private Movement movement;
    private MeshRenderer blackPlaneFader;
    private float timer = 0f;

    private Color transparent = new Color(0, 0, 0, 0);
    private Color black = new Color(0, 0, 0, 1);
    #endregion

    #region Init & Destruction
    protected override void InitObj() { }

    /// <summary>
    /// Initialization
    /// </summary>
    void Start()
    {
        //only initializes if isLocalPlayer
        if (isLocalPlayer)
        {
            fpsCamera = GetComponent<PlayerInitializer>().cameraToEnable;
            blackPlaneFader = fpsCamera.transform.GetChild(0).GetComponent<MeshRenderer>();
            movement = GetComponent<Movement>();

        }
    }
    #endregion

    #region Networking Function(s)
    /// <summary>
    /// Function to call from the server to force camera transition
    /// </summary>
    [ClientRpc]
    public void RpcSpawnRandom(Vector3 position)
    {
        if (!isServer)
            CanvasManager.Instance.SetMessage("Click one of the white boxes to choose where to spawn from");
        FadeOutAndIn(position);
    }
    #endregion

    #region Helper Functions
    /// <summary>
    /// Checks if user has clicked (selected) on an entrance to start at
    /// </summary>
    [Command]
    private void CmdSetIntruderMessage()
    {
        if (!isServer) return;
        CanvasManager.Instance.SetMessage("Intruder alert!");
    }

    /// <summary>
    /// Function to switch into placing mode
    /// </summary>
    private void FadeOutAndIn(Vector3 newPos)
    {
        if (!isLocalPlayer) return;

        StopAllCoroutines();

        if (movement)
            movement.DisableMovement();

        IEnumerator fadeOut = Fade(newPos); 
        timer = 0;
        StartCoroutine(fadeOut);
    }


    /// <summary>
    /// Fades out a camera and then switches it
    /// </summary>
    /// <param name="cameraToFadeOut">Either topViewCam or fpsCam</param>
    /// <param name="camToSwitchTo">To other cam to switch to</param>
    /// <param name="adjustTopViewCam">Whether or not the topViewCamera should adjust its position</param>
    /// <returns></returns>
    private IEnumerator Fade(Vector3 newPos)
    {
        while (timer < 1f)
        {
            timer += fadeSpeed;
            blackPlaneFader.material.SetColor("_Color", Color.Lerp(transparent, black, timer));
            yield return new WaitForEndOfFrame();
        }

        //waits before fading in
        yield return new WaitForSeconds(waitTime);
        transform.position = newPos;

        while (timer < 2f)
        {
            timer += fadeSpeed;
            blackPlaneFader.material.SetColor("_Color", Color.Lerp(black, transparent, timer  - 1f));
            yield return new WaitForEndOfFrame();
        }

        if (movement)
            movement.EnableMovement();

        CmdSetIntruderMessage();
        yield return null;
    }
    #endregion
}