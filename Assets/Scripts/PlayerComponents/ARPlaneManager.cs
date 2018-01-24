using System.Collections;
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
            //removes a plane from the list
            if (m_ARPlane.Count > UnityARAnchorManager.Instance.planeAnchorMap.Count)
            {
                for (int i = 0; i < m_ARPlane.Count; i++)
                {
                    if (!UnityARAnchorManager.Instance.planeAnchorMap.ContainsKey(m_ARPlane[i].identifier))
                    {
                        ServerRemovePlane(i);
                        break;
                    }
                }
            }

            float area = 0; //for calculation of  total area
            
            //go through all planes found by ARkit
            foreach (string s in UnityARAnchorManager.Instance.planeAnchorMap.Keys)
            {
                Transform plane = UnityARAnchorManager.Instance.planeAnchorMap[s].gameObject.transform.GetChild(0);

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
        ServerAddPlane("floor",
            Vector3.zero,
            Random.Range(0f, 360f),
            new Vector3(Random.Range(4f, 3f), 1f, Random.Range(4f, 3f)));

        for (int i = 0; i < 3; i++)
        {
            ServerAddPlane("table" + i,
               new Vector3(Random.Range(-0.75f, 0.75f), Random.Range(0.5f, 1.5f), Random.Range(-0.75f, 0.75f)),
               Random.Range(0f, 360f),
               new Vector3(Random.Range(2f, 0.25f), 1f, Random.Range(2f, 0.25f)));
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
