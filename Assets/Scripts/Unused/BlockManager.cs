using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.Networking;

#region SyncListStruct
public struct Block
{
    public Vector3 position;
    public int type;

    public Block(Vector3 position, int type)
    {
        this.position = position;
        this.type = type;
    }
}

public class BlockSync : SyncListStruct<Block> { }

#endregion
public class BlockManager : NetworkBehaviour
{
    //Sync Struct List
    public BlockSync blockList = new BlockSync();

    //if currently in placing mode
    bool isPlacing = false;
    bool prevIsPlacing;

    //if switching in/out of placing mode
    bool switching = false;

    //cameras
    private Camera vrCamera;
    public GameObject topViewCamPrefab;
    private GameObject topViewCamObj;
    private Camera topViewCam;

    private int currPlaceMode = -1;

    //the speed of fading
    [Range(0f, 1f)]
    public float fadeSpeed;

    //wait time between fade in/out
    public float waitTime = 1.2f;

    //prefab for placement visualization
    //public GameObject placementSamplePrefab;
    //private GameObject placementSampleObj;

    //mesh for placement visualization
    private Mesh[] blockMeshes;

    //player renderer
    public Renderer vrPlayerRenderer;
    private Movement movement;
    public LayerMask mask;

    public Vector3 startingPos;

    /// <summary>
    /// Initialization
    /// </summary>
    void Start()
    {
        //only initializes if isLocalPlayer
		if (isLocalPlayer) {
			prevIsPlacing = isPlacing;

			//creates top-view camera
			topViewCamObj = Instantiate (topViewCamPrefab);
			topViewCam = topViewCamObj.GetComponent<Camera> ();
			vrCamera = GetComponent<Player> ().VRCamera.GetComponent<Camera> ();

			//adjusts the black plane
			Color c = topViewCamObj.transform.GetChild (0).GetComponent<Renderer> ().material.color;
			c.a = 1f;
			topViewCamObj.transform.GetChild (0).GetComponent<Renderer> ().material.color = c;

			//placementSampleObj = Instantiate(placementSamplePrefab);
			//blockMeshes = LocalObjectBuilder.Instance.blockMeshes;
			vrPlayerRenderer = GetComponent<Player> ().VRAvatar.GetComponent<Renderer> ();

			movement = GetComponent<Movement> ();

			//CanvasManager.Instance.ToggleCrossHairUI();
			//StartPlacing();
			transform.position = new Vector3 (0.759f, 1001f, -0.659f);
			GameManager gameManager = FindObjectOfType<GameManager> ();
			if (gameManager && gameManager.CurrGamePhase == GamePhase.Playing) {
				StartPlacing ();
			} else {
				movement.SwitchToPlaying ();
			}
		}

        else if (isServer)
        {
            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager && gameManager.CurrGamePhase == GamePhase.Playing)
            {
                RpcStartPlacing();
            }
        }

        //LocalObjectBuilder.Instance.SetBlockManager(this);
    }

    private void OnDestroy()
    {
        if (topViewCamObj)
        {
            Destroy(topViewCamObj);
        }

        //if (placementSampleObj)
        //{
        //    Destroy(placementSampleObj);
        //}
    }

    #region Getter For Unity Actions
    /*
    /// <summary>
    /// Gets an action to attach to a button
    /// </summary>
    /// <param name="i">Index of the button</param>
    /// <param name="placementBtn">Button for placement</param>
    /// <returns></returns>
    public UnityAction GetActionToSetPlacingMode(int i, Button placementBtn)
    {
        //creates the action
        UnityAction action = () =>
        {
            //if already on current placemode
            if (currPlaceMode == i)
            {
                placementSampleObj.GetComponent<MeshFilter>().mesh = null;
                currPlaceMode = -1;
                placementBtn.interactable = false;
            }

            //switches to desired placemode
            else
            {
                placementSampleObj.GetComponent<MeshFilter>().mesh = blockMeshes[i];
                currPlaceMode = i;
                placementBtn.interactable = true;
            }
        };

        return action;
    }

    /// <summary>
    /// Function callback for placing blocks
    /// </summary>
    /// <param name="btns">Array of buttons</param>
    /// <param name="placementBtn">The main placement button</param>
    /// <returns></returns>
    public UnityAction GetActionToPlaceBlock(Button[] btns, Button placementBtn)
    {
        //Creates the action
        UnityAction action = () =>
        {
            if (currPlaceMode != -1)
            {
                btns[currPlaceMode].interactable = false;
                placementBtn.interactable = false;
                placementSampleObj.GetComponent<MeshFilter>().mesh = null;

                CmdAddBlock(placementSampleObj.transform.position, currPlaceMode);

                currPlaceMode = -1;
            }
        };

        return action;
    }
    */
    #endregion

    // Update is called once per frame
    void Update()
    {
        //Exits if it isn't local player
        if (!isLocalPlayer)
            return;

        //Makes it so that you can't do anything while switching
        UpdateSwitching();
        if (switching)
            return;

        if (transform.position.y < -3f)
        {
            GetComponent<Combat>().TakeDamage();
            transform.position = startingPos;
        }

        //Input to switch into or out of placing mode
        if (Input.GetKeyDown(KeyCode.T) && Input.GetKey(KeyCode.LeftShift))
        {
            if (!isPlacing)
                StartPlacing();
            else StopPlacing();
        }

        if (isPlacing)
        {
            //check if an entrance was hit
            if (Input.GetMouseButtonUp(0))
            {
                RaycastHit hitInfo;
                Ray ray = topViewCam.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hitInfo, 2000f, mask))
                {
                    Debug.Log(hitInfo.collider.name);
                    StopPlacing();
                    transform.position = hitInfo.point;
                    startingPos = hitInfo.point;
                }
            }

            float zoom = -Input.GetAxis("Mouse ScrollWheel");
            float horizontal = Input.GetAxis("Mouse X");
            float vertical = Input.GetAxis("Mouse Y");

            horizontal *= 0.02f;
            vertical *= 0.02f;


            topViewCam.transform.position = topViewCam.transform.position +
                horizontal * topViewCam.transform.right +
                vertical * topViewCam.transform.up;
            topViewCam.orthographicSize += zoom;
        }

        //if (isPlacing && currPlaceMode > -1)
        //{
        //    //gets mouse position
        //    Vector3 mousePos = Input.mousePosition;
        //    mousePos.z = topViewCamObj.transform.position.y;
        //    mousePos = topViewCam.ScreenToWorldPoint(mousePos);
        //    mousePos.y = 0;

        //    //sets the position of the placement sample
        //    placementSampleObj.transform.position = mousePos;
        //}
    }

    #region Helper Functions

    /// <summary>
    /// Helper function for checking if switching between modes
    /// </summary>
    void UpdateSwitching()
    {
        if (prevIsPlacing != isPlacing)
            switching = false;

        prevIsPlacing = isPlacing;
    }

    /// <summary>
    /// Function to switch into placing mode
    /// </summary>
    void StartPlacing()
    {
        if (!isLocalPlayer || isPlacing) return;

        StopAllCoroutines();
        switching = true;
        if (movement)
            movement.SwitchOutOfPlaying();

        IEnumerator fadeOut = FadeOut(vrCamera, topViewCam, true);
        IEnumerator fadeIn = FadeIn(topViewCam, vrCamera);

        StartCoroutine(fadeOut);
        StartCoroutine(fadeIn);
    }

    /// <summary>
    /// Function to switch out of placing mode
    /// </summary>
    void StopPlacing()
    {
        if (!isLocalPlayer || !isPlacing) return;

        StopAllCoroutines();
        switching = true;

        IEnumerator fadeOut = FadeOut(topViewCam, vrCamera, false);
        IEnumerator fadeIn = FadeIn(vrCamera, topViewCam);

        StartCoroutine(fadeIn);
        StartCoroutine(fadeOut);
    }

    IEnumerator FadeOut(Camera cameraToFadeOut, Camera otherCamera, bool reposition)
    {
        while (otherCamera.enabled)
        {
            //Debug.Log("Other Camera (" + otherCamera.name + ") is still active");
            yield return null;
        }

        Renderer rend = cameraToFadeOut.transform.GetChild(0).GetComponent<Renderer>();
        Color c = rend.material.color;
        float alpha = rend.material.color.a;

        for (; alpha < 1f; alpha += fadeSpeed)
        {
            c.a = alpha;
            rend.material.color = c;

            yield return null;
        }

        c.a = 1;
        rend.material.color = c;

        if (reposition)
        {
            Vector3 avgPos = LocalObjectBuilder.Instance.GetAveragePos();
            Vector3 topViewPos = otherCamera.transform.position;
            topViewPos.x = avgPos.x;
            topViewPos.z = avgPos.z;
            otherCamera.transform.position = topViewPos;
        }

        yield return new WaitForSeconds(waitTime);

        cameraToFadeOut.enabled = false;
        otherCamera.enabled = true;

        if (isPlacing)
        {
            //vrPlayerRenderer.enabled = false;
            //CanvasManager.Instance.DisableBlockPlacingUI();
            //CanvasManager.Instance.ToggleCrossHairUI();
        }
    }

    IEnumerator FadeIn(Camera cameraToFadeIn, Camera otherCamera)
    {
        while (otherCamera.enabled)
        {
            yield return null;
        }

        Renderer rend = cameraToFadeIn.transform.GetChild(0).GetComponent<Renderer>();
        Color c = rend.material.color;
        float alpha = rend.material.color.a;

        for (; alpha > 0f; alpha -= fadeSpeed)
        {
            c.a = alpha;
            rend.material.color = c;

            yield return null;
        }

        c.a = 0;
        rend.material.color = c;

        isPlacing = !isPlacing;
        if (isPlacing)
        {
            //vrPlayerRenderer.enabled = true;
            SetUpUI();
        }
        else
        {
            if (movement)
                movement.SwitchToPlaying();

            Combat combat = GetComponent<Combat>();
            if (combat)
            {
                combat.InitHealthBar();
            }
        }
    }

    /// <summary>
    /// Helper function for Setting Up UI
    /// </summary>
    void SetUpUI()
    {
        //CanvasManager.Instance.SetUpBlockPlacingUI(this, blockMeshes.Length);
        //CanvasManager.Instance.ToggleCrossHairUI();
    }
    #endregion

    #region Networking Function(s)
    [Command]
    private void CmdAddBlock(Vector3 position, int type)
    {
        blockList.Add(new Block(position, type));
    }

    [ClientRpc]
    public void RpcStartPlacing()
    {
        StartPlacing();
    }
    #endregion
}
