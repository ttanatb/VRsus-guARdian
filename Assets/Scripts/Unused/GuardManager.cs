using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.iOS;
using UnityEngine.EventSystems;

public class GuardManager : SingletonMonoBehaviour<GuardManager>
{
    public GameObject guardPrefab;      //used for instantiating
    private GameObject VRPlayerAvatar;  //used for checking (distance and stuff)

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //check if it's the correct game phase
        if (UnityARAnchorManager.Instance.GetAnchorCount() > 0)
        {
            //check if there's a touch input
            if (Input.touchCount > 0)
            {
                //check if touch input is over UI object
                if (IsPointerOverUIObject()) return;

                //loop through all touches
                foreach (Touch t in Input.touches)
                {
                    //only check for taps
                    if (t.phase == TouchPhase.Began)
                    {
                        //get touch position and raycast from there
                        Vector3 vec = Camera.main.ScreenToWorldPoint(t.position);
                        RaycastHit hit;
                        if (Physics.Raycast(vec, Camera.main.transform.forward, out hit))           //should have a dist for raycasting
                        {
                            if (hit.collider.gameObject.tag == "Platform")
                            {
                                GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                                obj.transform.position = hit.point;

                                //change to spawn
                            }
                        }
                    }
                }
            }
        }

    }

    private bool CheckTouch()
    {
        return false;
    }

    private bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    private List<ARHitTestResult> GetARHitResults(Touch touch)
    {
        Vector3 screenPosition = Camera.main.ScreenToViewportPoint(touch.position);
        ARPoint point = new ARPoint
        {
            x = screenPosition.x,
            y = screenPosition.y
        };

        return UnityARSessionNativeInterface.GetARSessionNativeInterface().HitTest(point,
            ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingExtent);
    }
}
