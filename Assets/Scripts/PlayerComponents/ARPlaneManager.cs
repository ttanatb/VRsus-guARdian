using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.iOS;
using UnityEngine.Networking;

/// <summary>
/// Builds a list of planes that is linked across the two players
/// 
/// Author: Tanat Boozayaangool
/// </summary>
public class ARPlaneManager : PlayerComponent
{
    //The list of planes (as a list of synced structs)
    public ARPlaneSync m_ARPlane = new ARPlaneSync();

#if !UNITY_IOS
    [MinMaxSlider(2f, 7f)]
    public Vector2 roomWidthRange = new Vector2(2f, 5f);
    [MinMaxSlider(2f, 7f)]
    public Vector2 roomLengthRange = new Vector2(3f, 7f);

    public int planeCount = 4;

    [MinMaxSlider(0.1f, 0.8f)]
    public Vector2 planeElevationRange = new Vector2(0.25f, 0.5f);

    [MinMaxSlider(0.1f, 3f)]
    public Vector2 planeWidthRange = new Vector2(.25f, 2f);
    [MinMaxSlider(0.1f, 3f)]
    public Vector2 planeLengthRange = new Vector2(.25f, 2f);
#endif

    protected override void InitObj() { }

    public override void OnStartLocalPlayer()
    {
        if (isServer)
        {
            StartCoroutine(UpdateARPlanes());
        }
    }

    private void Start()
    {
        if (LocalObjectBuilder.Instance)
            LocalObjectBuilder.Instance.SetPlaneManager(this);
    }

    private void OnDestroy()
    {
        if (LocalObjectBuilder.Instance && LocalObjectBuilder.Instance.Manager == this)
            LocalObjectBuilder.Instance.Clear();
    }

    #region Server Functions
    /// <summary>
    /// Reads the planes found on AR and updates the list accordingly
    /// </summary>
    [Server]
    IEnumerator UpdateARPlanes()
    {
#if UNITY_IOS
        while(true)
        {
            if (UnityARAnchorManager.Instance == null) {
                yield return new WaitForSeconds(1f);
            }

            //removes a plane from the list
            if (m_ARPlane.Count > UnityARAnchorManager.Instance.PlaneAnchorMap.Count)
            {
                for (int i = 0; i < m_ARPlane.Count; i++)
                {
                    if (!UnityARAnchorManager.Instance.PlaneAnchorMap.ContainsKey(m_ARPlane[i].identifier))
                    {
                        ServerRemovePlane(i);
                        break;
                    }
                }
            }

            float area = 0; //for calculation of  total area
            
            //go through all planes found by ARkit
            foreach (string s in UnityARAnchorManager.Instance.PlaneAnchorMap.Keys)
            {
                Transform plane = UnityARAnchorManager.Instance.PlaneAnchorMap[s].gameObject.transform.GetChild(0);

                //if plane has already been added
                if (CheckIfContains(s))
                {
                    int index = GetIndexOf(s);
                    if (index != -1)
                    {
                        area += plane.localScale.x * plane.localScale.z;
                        ServerUpdatePlane(index, plane.position, plane.rotation.eulerAngles.y, plane.localScale * 10);
                    }
                }
                else
                {
                    //adds the newly found plane
                    ServerAddPlane(s, plane.position, plane.rotation.eulerAngles.y, plane.localScale * 10);
                }
            }

            CanvasManager.Instance.UpdateTotalPlaneArea(area * 100f);
            yield return new WaitForSeconds(1f);
        }
#else
        //build planes to simulate a random room
        float width = Random.Range(roomWidthRange.x, roomWidthRange.y);
        float length = Random.Range(roomLengthRange.x, roomLengthRange.y);
        float rotY = Random.Range(0f, 360f);

        ServerAddPlane("floor", Vector3.zero, rotY, new Vector3(width, 1f, length));
        List<Vector3> floorGeometry = Utility.CreateVerticesFromPlane(Vector3.zero, (new Vector2(width, length)) / 2f, rotY);

        List<List<Vector3>> prevPlanes = new List<List<Vector3>>();// { floorGeometry };
        for (int i = 0; i < planeCount; i++)
        {
            Vector3 planePos = Utility.GetRandomPointInPlane(floorGeometry);
            for (int j = 0; j < prevPlanes.Count; j++)
            {
                if (Utility.CheckIfPointIsInPolygon(planePos, prevPlanes[j]))
                {
                    planePos = Utility.GetRandomPointInPlane(floorGeometry);
                    j = -1;
                }
            }

            planePos += Vector3.up * (((planeElevationRange.y - planeElevationRange.x) * (i + 1) / 0.75f)  + Random.Range(planeElevationRange.x, planeElevationRange.y)); // (Random.Range(0.2f, planeElevationRange.y - planeElevationRange.x) * i / planeCount) + planeElevationRange.x);
            float planeRotY = Random.Range(0f, 360f);
            Vector3 planeScale = new Vector3(Random.Range(planeWidthRange.x, planeWidthRange.y), 1f, Random.Range(planeLengthRange.x, planeLengthRange.y));

            List<Vector3> planeGeometry = Utility.CreateVerticesFromPlane(planePos, (new Vector2(planeScale.x, planeScale.z)) / 2f, rotY);
            prevPlanes.Add(planeGeometry);
            ServerAddPlane("table " + i, planePos, planeRotY, planeScale);
        }

        yield return null;
#endif
    }

    /// <summary>
    /// Adds a new plane to the list
    /// </summary>
    /// <param name="s">The identifier (given from ARkit)</param>
    /// <param name="pos">The position of the plane</param>
    /// <param name="rot">The rotation of the plane</param>
    /// <param name="scale">The scale of the plane</param>
    [Server]
    private void ServerAddPlane(string s, Vector3 pos, float rot, Vector3 scale)
    {
        m_ARPlane.Add(new ARPlane(s, pos, rot, scale));
        CanvasManager.Instance.UpdatePlaneCount(m_ARPlane.Count);
    }

    /// <summary>
    /// Updates the data of a given plane
    /// </summary>
    /// <param name="index">The index of the plane in the list</param>
    /// <param name="pos">The new position of the plane</param>
    /// <param name="rot">The new rotation of the plane</param>
    /// <param name="scale">The new scale of the plane</param>
    [Server]
    private void ServerUpdatePlane(int index, Vector3 pos, float rot, Vector3 scale)
    {
        if (index < m_ARPlane.Count)
        {
            //must create a new SyncStruct to make sure it updates on client
            m_ARPlane[index] = new ARPlane(m_ARPlane[index].identifier, pos, rot, scale);
            m_ARPlane.Dirty(index);
        }
    }

    /// <summary>
    /// Removes a plane from a list
    /// </summary>
    /// <param name="index">index to remove from</param>
    [Server]
    private void ServerRemovePlane(int index)
    {
        m_ARPlane.RemoveAt(index);
        CanvasManager.Instance.UpdatePlaneCount(m_ARPlane.Count);
    }

    #endregion

    #region Helper Functions
    /// <summary>
    /// Checks if m_ARPlane contains a plane with the string identifier
    /// </summary>
    private bool CheckIfContains(string identifier)
    {
        for (int i = 0; i < m_ARPlane.Count; i++)
        {
            if (m_ARPlane[i].identifier == identifier)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Gets an index of an ARPlane in m_ARPlane based off of the identifier
    /// </summary>
    private int GetIndexOf(string identifier)
    {
        for (int i = 0; i < m_ARPlane.Count; i++)
        {
            if (m_ARPlane[i].identifier == identifier)
            {
                return i;
            }
        }

        return -1;
    }
    #endregion
}
