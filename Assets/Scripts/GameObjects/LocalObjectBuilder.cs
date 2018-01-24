using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This builds the planes based off of the networked data.
/// This also keep tracks of data around planes
/// 
/// Author: Tanat Boozayaangool
/// </summary>
public class LocalObjectBuilder : SingletonMonoBehaviour<LocalObjectBuilder>
{
    #region Fields
    [Tooltip("The prefab for planes")]
    public GameObject planePrefab;

    //local plane objects
    [SerializeField]
    private List<GameObject> localPlanes;
    private int prevPlanesListCount = 0;

    //the AR object that networks the detected planes
    private ARPlaneManager planeManager;

    private float floorPos;     //y-pos of floor

    /// <summary>
    /// Y-position of the floor
    /// </summary>
    public float FloorPos { get { return floorPos; } }

    public ARPlaneManager Manager { get { return planeManager; } }
    #endregion

    #region Init & Destructor
    // Use this for initialization
    void Start()
    {
        Init();
    }

    /// <summary>
    /// Initialization logic
    /// </summary>
    private void Init()
    {
        localPlanes = new List<GameObject>();
    }

    /// <summary>
    /// Constructor of sorts to set upt the correct variables
    /// </summary>
    /// <param name="planeManager"></param>
    public void SetPlaneManager(ARPlaneManager planeManager)
    {
        //Debug.Log("Setting plane manager");
        this.planeManager = planeManager;
        Clear();
        Init();
        StartCoroutine("UpdateLocalPlanes");
    }

    // Important for singleton logic
    protected override void OnDestroy()
    {
        Clear();
        base.OnDestroy();
    }

    /// <summary>
    /// Clears out all the planes
    /// </summary>
    public void Clear()
    {
        //Debug.Log("Clearing");
        StopAllCoroutines();
        for (int i = 0; i < localPlanes.Count; i++)
            Destroy(localPlanes[i]);

        localPlanes.Clear();
    }
    #endregion

    #region Helper Functions & Life Cycle
    /// <summary>
    /// Gets the average position between all planes
    /// </summary>
    /// <returns>The average position for all planes</returns>
    public Vector3 GetAveragePos()
    {
        Vector3 avgPos = Vector3.zero;
        for (int i = 0; i < localPlanes.Count; i++)
            avgPos += localPlanes[i].transform.position;

        if (localPlanes.Count > 0)
            return avgPos / localPlanes.Count;
        else return avgPos;
    }

    /// <summary>
    /// Reads the list from the plane manager and builds it
    /// </summary>
    IEnumerator UpdateLocalPlanes()
    {
        //endless loop
        for (; ; )
        {
            //add a plane
            if (prevPlanesListCount < planeManager.m_ARPlane.Count)
            {
                GameObject obj = Instantiate(planePrefab);
                localPlanes.Add(obj);
            }

            //destory a plane
            else if (prevPlanesListCount > planeManager.m_ARPlane.Count)
            {
                Destroy(localPlanes[prevPlanesListCount - 1]);
                localPlanes.RemoveAt(prevPlanesListCount - 1);
            }

            //update all the planes
            for (int i = 0; i < localPlanes.Count; i++)
            {
                //check to make sure plane exists
                if (i < planeManager.m_ARPlane.Count)
                {
                    localPlanes[i].GetComponent<LocalPlane>().UpdatePos(planeManager.m_ARPlane[i].position,
                        planeManager.m_ARPlane[i].rotation,
                        planeManager.m_ARPlane[i].scale);

                    float yPos = planeManager.m_ARPlane[i].position.y;

                    floorPos = yPos < floorPos ? yPos : floorPos;
                }
                else
                    break;
            }

            prevPlanesListCount = localPlanes.Count;
            yield return new WaitForSeconds(.1f);
        }
    }
    #endregion
}