using System.Collections;
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
    private bool isUsingTopViewCam = false;         //if currently in placing mode
    private bool switchingCam = false;               //if switching in/out of placing mode

    //cameras
    public GameObject topViewCamPrefab;
    private GameObject topViewCamObj;

    private Camera topViewCam;
    private Camera fpsCamera;

    //the speed of fading
    [Range(0f, 1f)]
    public float fadeSpeed;

    //wait time between fade in/out
    public float waitTime = 1.2f;

    //player renderer
    private Movement movement;
    public LayerMask mask;

    public Vector3 startingPos;
    #endregion

    #region Init & Destruction
    protected override void InitObj()
    {
        if (!topViewCamObj)
            topViewCamObj = Instantiate(topViewCamPrefab);

        if (!topViewCam && topViewCamObj)
            topViewCam = topViewCamObj.GetComponent<Camera>();
    }

    /// <summary>
    /// Initialization
    /// </summary>
    void Start()
    {
        //only initializes if isLocalPlayer
        if (isLocalPlayer)
        {
            InitObj(); //creates top-view camera
            fpsCamera = GetComponent<PlayerInitializer>().VRCamera.GetComponent<Camera>();

            //adjusts the black plane
            Color c = topViewCamObj.transform.GetChild(0).GetComponent<Renderer>().material.color;
            c.a = 1f;
            topViewCamObj.transform.GetChild(0).GetComponent<Renderer>().material.color = c;

            movement = GetComponent<Movement>();

            SwitchToTopViewCam();
            Invoke("StopPlacing", 1.75f);
            transform.position = new Vector3(0.759f, 1005f, -0.659f);
            movement.SwitchToPlaying();
        }
    }

    private void OnDestroy()
    {
        if (topViewCamObj)
        {
            Destroy(topViewCamObj);
        }
    }
    #endregion

    #region Life Cycle
    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer || switchingCam || !isUsingTopViewCam)
            return;

        CheckClickOnEntrance();
        UpdateTopViewCameraMovement();
    }
    #endregion

    #region Networking Function(s)
    /// <summary>
    /// Function to call from the server to force camera transition
    /// </summary>
    [ClientRpc]
    public void RpcSwitchToTopViewCam()
    {
        SwitchToTopViewCam();
    }
    #endregion

    #region Helper Functions
    /// <summary>
    /// Checks if user has clicked (selected) on an entrance to start at
    /// </summary>
    private void CheckClickOnEntrance()
    {
        //check left click
        if (Input.GetMouseButtonUp(0))
        {
            //raycasts from click
            RaycastHit hitInfo;
            Ray ray = topViewCam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hitInfo, float.MaxValue, mask))
            {
                //switches to the location
                SwitchToFPSCam();
                transform.position = hitInfo.point;
                startingPos = hitInfo.point;
            }
        }
    }

    /// <summary>
    /// Moves the screen around according to mouse movement
    /// </summary>
    private void UpdateTopViewCameraMovement()
    {
        float zoom = -Input.GetAxis("Mouse ScrollWheel");
        float horizontal = Input.GetAxis("Mouse X");
        float vertical = Input.GetAxis("Mouse Y");

        horizontal *= 0.02f;
        vertical *= 0.02f;

        //adjusts cam position and size
        topViewCam.transform.position = topViewCam.transform.position +
            horizontal * topViewCam.transform.right +
            vertical * topViewCam.transform.up;
        topViewCam.orthographicSize += zoom;
    }

    /// <summary>
    /// Function to switch into placing mode
    /// </summary>
    private void SwitchToTopViewCam()
    {
        if (!isLocalPlayer || isUsingTopViewCam) return;

        StopAllCoroutines();
        switchingCam = true;

        if (movement)
            movement.SwitchOutOfPlaying();

        IEnumerator fadeOut = FadeOut(fpsCamera, topViewCam, true);
        IEnumerator fadeIn = FadeIn(topViewCam, fpsCamera);

        StartCoroutine(fadeOut);
        StartCoroutine(fadeIn);
    }

    /// <summary>
    /// Function to fade cameras and switch to a FPS camera
    /// </summary>
    private void SwitchToFPSCam()
    {
        if (!isLocalPlayer || !isUsingTopViewCam) return;

        StopAllCoroutines();
        switchingCam = true;

        IEnumerator fadeOut = FadeOut(topViewCam, fpsCamera, false);
        IEnumerator fadeIn = FadeIn(fpsCamera, topViewCam);

        StartCoroutine(fadeIn);
        StartCoroutine(fadeOut);
    }

    /// <summary>
    /// Fades out a camera and then switches it
    /// </summary>
    /// <param name="cameraToFadeOut">Either topViewCam or fpsCam</param>
    /// <param name="camToSwitchTo">To other cam to switch to</param>
    /// <param name="adjustTopViewCam">Whether or not the topViewCamera should adjust its position</param>
    /// <returns></returns>
    private IEnumerator FadeOut(Camera cameraToFadeOut, Camera camToSwitchTo, bool adjustTopViewCam)
    {
        //gets the 'screen' in front of the camera to fake the fading
        Renderer rend = cameraToFadeOut.transform.GetChild(0).GetComponent<Renderer>();
        Color c = rend.material.color;
        float alpha = rend.material.color.a;

        //adjusts the alpha to fade
        for (; alpha < 1f; alpha += fadeSpeed)
        {
            c.a = alpha;
            rend.material.color = c;

            yield return null;
        }

        //sets it at 1
        c.a = 1;
        rend.material.color = c;

        //repositions the top-view camera
        if (adjustTopViewCam)
        {
            Vector3 avgPos = LocalObjectBuilder.Instance.GetAveragePos();
            Vector3 topViewPos = camToSwitchTo.transform.position;
            topViewPos.x = avgPos.x;
            topViewPos.z = avgPos.z;
            camToSwitchTo.transform.position = topViewPos;
        }

        //waits before fading in
        yield return new WaitForSeconds(waitTime);

        //switches the camera
        cameraToFadeOut.enabled = false;
        camToSwitchTo.enabled = true;
    }

    /// <summary>
    /// Fades in a camera and then updates variables
    /// </summary>
    /// <param name="cameraToFadeIn">Either topViewCam or fpsCam</param>
    /// <param name="otherCamera">The other cam to switch from</param>
    private IEnumerator FadeIn(Camera cameraToFadeIn, Camera otherCamera)
    {
        //waits until camera had switched
        while (otherCamera.enabled)
        {
            yield return null;
        }

        //gets the 'screen' in front of the camera to fake the fading
        Renderer rend = cameraToFadeIn.transform.GetChild(0).GetComponent<Renderer>();
        Color c = rend.material.color;
        float alpha = rend.material.color.a;

        //adjusts the alpha to fade
        for (; alpha > 0f; alpha -= fadeSpeed)
        {
            c.a = alpha;
            rend.material.color = c;

            yield return null;
        }

        //sets it at 0
        c.a = 0;
        rend.material.color = c;

        //finalizes switch
        isUsingTopViewCam = !isUsingTopViewCam;
        switchingCam = false;
        if (!isUsingTopViewCam)
        {
            if (movement)
                movement.SwitchToPlaying();
        }
    }
    #endregion
}