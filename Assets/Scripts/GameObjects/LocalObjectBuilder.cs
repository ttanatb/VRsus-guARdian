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
    /// Returns all the points defining the world bounds
    /// </summary>
    /// <returns>List of vector3 that define the bounds of the world</returns>
    public List<Vector3> GetWorldGeometry()
    {
        List<Vector3> worldGeometry = new List<Vector3>();
        float largestArea = float.MinValue;
        int indexOfLargestPlane = -1;

        //loop through planes to find largest plane
        for (int i = 0; i < localPlanes.Count; i++)
        {
            float planeArea = localPlanes[i].transform.localScale.x * localPlanes[i].transform.localScale.z;
            if (planeArea > largestArea)
            {
                largestArea = planeArea;
                indexOfLargestPlane = i;
            }
        }
        if (indexOfLargestPlane < 0) return worldGeometry;  //if there is no plane, return empty

        //add the corners of the plane;
        GameObject largestPlane = localPlanes[indexOfLargestPlane];
        Vector3 planeScale = largestPlane.transform.localScale / 2f;
        worldGeometry.Add(new Vector3(planeScale.x, 0f, planeScale.z));
        worldGeometry.Add(new Vector3(-planeScale.x, 0f, planeScale.z));
        worldGeometry.Add(new Vector3(-planeScale.x, 0f, -planeScale.z));
        worldGeometry.Add(new Vector3(planeScale.x, 0f, -planeScale.z));

        //loop through to rotate and translate accordingly
        for (int i = 0; i < worldGeometry.Count; i++)
            worldGeometry[i] = (Quaternion.AngleAxis(largestPlane.transform.eulerAngles.y, Vector3.up) * worldGeometry[i]) + largestPlane.transform.position;

        return worldGeometry;
    }

    public List<List<Vector3>> GetSortedPlanes()
    {
        List<List<Vector3>> sortedList = new List<List<Vector3>>();
        for(int i = 0; i < localPlanes.Count; i++)
        {
            int insertionIndex = 0;
            float yPos = localPlanes[i].transform.position.y;
            for(; insertionIndex < sortedList.Count; insertionIndex++)
            {
                float thisYPos = sortedList[insertionIndex][0].y; //.transform.position.y;
                if (yPos < thisYPos)
                    break;
            }
            sortedList.Insert(insertionIndex, Utility.CreateVerticesFromPlane(localPlanes[i]));
        }

        return sortedList;
    }

    //for testing
    private void Update()
    {
        //List<Vector3> worldGeometry = GetWorldGeometry();
        //for (int i = 0; i < worldGeometry.Count; i++)
        //{
        //    Vector3 point1 = worldGeometry[i];
        //    Vector3 point2 = Vector3.zero;
        //    if (i == worldGeometry.Count - 1) point2 = worldGeometry[0];
        //    else point2 = worldGeometry[i + 1];
        //    Debug.DrawLine(point1, point2);
        //}

        //if (Input.GetKeyDown(KeyCode.D))
        //    StopAllCoroutines();
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