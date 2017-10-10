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
    public BlockSync blockList = new BlockSync();
    bool isPlacing = false;
    bool prevIsPlacing;

    bool switching = false;

    private GameObject vrCamObj;
    public GameObject topViewCamObj;
    private Camera topViewCam;

    private int currPlaceMode = -1;

    [Range(0f, 1f)]
    public float fadeSpeed;

    public float waitTime = 1.2f;

    public GameObject placementSamplePrefab;
    private GameObject placementSampleObj;

    private Mesh[] blockMeshes;

    // Use this for initialization
    void Start()
    {
        if (isLocalPlayer)
        {
            topViewCamObj = Instantiate(topViewCamObj);
            topViewCam = topViewCamObj.GetComponent<Camera>();

            Color c = topViewCamObj.transform.GetChild(0).GetComponent<Renderer>().material.color;
            c.a = 1f;
            topViewCamObj.transform.GetChild(0).GetComponent<Renderer>().material.color = c;

            vrCamObj = Camera.main.gameObject;
            prevIsPlacing = isPlacing;

            placementSampleObj = Instantiate(placementSamplePrefab);
            blockMeshes = LocalObjectBuilder.Instance.blockMeshes;

            LocalObjectBuilder.Instance.SetBlockManager(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer)
            return;

        UpdateSwitching();
        if (switching)
            return;

        if (Input.GetKeyDown(KeyCode.T) && Input.GetKey(KeyCode.LeftShift))
        {
            if (!isPlacing)
                StartPlacing();
            else StopPlacing();
        }

        if (isPlacing && currPlaceMode > -1)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = topViewCamObj.transform.position.y;
            mousePos = topViewCam.ScreenToWorldPoint(mousePos);

            mousePos.y = 0;

            placementSampleObj.transform.position = mousePos;
        }
    }

    void UpdateSwitching()
    {
        if (prevIsPlacing != isPlacing)
            switching = false;

        prevIsPlacing = isPlacing;
    }

    void StartPlacing()
    {
        if (isPlacing) return;

        StopAllCoroutines();
        switching = true;

        IEnumerator fadeOut = FadeOut(vrCamObj, topViewCamObj, true);
        IEnumerator fadeIn = FadeIn(topViewCamObj, vrCamObj);

        //Debug.Log("Switching to placing mode");
        StartCoroutine(fadeOut);
        StartCoroutine(fadeIn);
    }

    void StopPlacing()
    {
        if (!isPlacing) return;

        StopAllCoroutines();
        switching = true;

        IEnumerator fadeOut = FadeOut(topViewCamObj, vrCamObj, false);
        IEnumerator fadeIn = FadeIn(vrCamObj, topViewCamObj);


        //Debug.Log("Switching out of placing mode");
        StartCoroutine(fadeIn);
        StartCoroutine(fadeOut);
    }

    void SetUpUI()
    {
        CanvasManager.Instance.SetUpBlockPlacingUI(this, blockMeshes.Length);
    }

    public UnityAction GetActionToSetPlacingMode(int i, Button placementBtn)
    {
        UnityAction action = () =>
        {
            if (currPlaceMode == i)
            {
                placementSampleObj.GetComponent<MeshFilter>().mesh = null;
                currPlaceMode = -1;
                placementBtn.interactable = false;
            }
            else
            {
                placementSampleObj.GetComponent<MeshFilter>().mesh = blockMeshes[i];
                currPlaceMode = i;
                placementBtn.interactable = true;
            }

            Debug.Log(currPlaceMode);
        };

        return action;
    }

    public UnityAction GetActionToPlaceBlock(Button[] btns, Button placementBtn)
    {
        UnityAction action = () =>
        {
            btns[currPlaceMode].interactable = false;
            placementBtn.interactable = false;
            placementSampleObj.GetComponent<MeshFilter>().mesh = null;

            //run the thing to the server
            ServerAddBlock(placementSampleObj.transform.position, currPlaceMode);

            currPlaceMode = -1;
        };

        return action;
    }

    [Server]
    private void ServerAddBlock(Vector3 position, int type)
    {
        blockList.Add(new Block(position, type));
    }

    IEnumerator FadeOut(GameObject cameraToFadeOut, GameObject otherCamera, bool reposition)
    {
        while (otherCamera.activeSelf)
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

        cameraToFadeOut.SetActive(false);
        otherCamera.SetActive(true);
        if (isPlacing)
            CanvasManager.Instance.DisableBlockPlacingUI();
    }

    IEnumerator FadeIn(GameObject cameraToFadeIn, GameObject otherCamera)
    {
        while (otherCamera.activeSelf)
        {
            //Debug.Log("Other Camera (" + otherCamera.name + ") is still active");
            yield return null;
        }

        //Debug.Log("Fading in now!");

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
            SetUpUI();
    }
}
