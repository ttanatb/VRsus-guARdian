using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.iOS;
using UnityEngine.Networking;

#region SyncListStruct
public struct ARPlane
{
    public string identifier;
    public Vector3 position;
    public float rotation;
    public Vector3 scale;

    public ARPlane(string identifier, Vector3 position, float rotation, Vector3 scale)
    {
        this.identifier = identifier;
        this.position = position;
        this.rotation = rotation;
        this.scale = scale;
    }

    //public void Update(Vector3 position, float rotation, Vector3 scale)
    //{
    //    this.position = position;
    //    this.rotation = rotation;
    //    this.scale = scale;
    //}
}

public class ARPlaneSync : SyncListStruct<ARPlane> { }
#endregion

public class PlaneManager : NetworkBehaviour
{
    public ARPlaneSync m_ARPlane = new ARPlaneSync();

    enum GamePhase
    {
        Scanning = 0,
        Playing = 1
    }

    //[SyncVar]
    //int currGamePhase = (int)GamePhase.Scanning;

    private Player player;

    // Use this for initialization
    void Start()
    {
        player = GetComponent<Player>();
        if (player.PlayerType == PlayerType.AR)
        {

#if UNITY_IOS
            if (isServer)
            {
                StartCoroutine("UpdateARPlanes");
            }
#endif
            LocalObjectBuilder.Instance.SetPlaneManager(this);
        }
    }


    private void Update()
    {
        if (isServer)
        {
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.D))
            {
                ServerAddPlane("dfdf",
                    new Vector3(Random.Range(-5f, 5f), Random.Range(-3f, 1f), Random.Range(-6f, 4f)),
                    Random.Range(0f, 360f),
                    new Vector3(Random.Range(3f, 6f), Random.Range(3f, 6f), Random.Range(3f, 6f)));
            }
        }
    }

    #region Server Functions
    [Server]
    IEnumerator UpdateARPlanes()
    {
        if (!isServer)
            yield return null;
        for (; ; )
        {
            if (m_ARPlane.Count > UnityARAnchorManager.Instance.planeAnchorMap.Count)
            {
                for (int i = 0; i < m_ARPlane.Count; i++)
                {
                    if (!UnityARAnchorManager.Instance.planeAnchorMap.ContainsKey(m_ARPlane[i].identifier))
                    {
                        //ServerRemovePlane(i);
                        break;
                    }
                }
            }

            foreach (string s in UnityARAnchorManager.Instance.planeAnchorMap.Keys)
            {
                if (CheckIfContains(s))
                {
                    int index = GetIndexOf(s);
                    if (index != -1)
                    {
                        ServerUpdatePlane(index,
                            UnityARAnchorManager.Instance.planeAnchorMap[s].gameObject.transform.GetChild(0).position,
                            UnityARAnchorManager.Instance.planeAnchorMap[s].gameObject.transform.GetChild(0).rotation.eulerAngles.y,
                            UnityARAnchorManager.Instance.planeAnchorMap[s].gameObject.transform.GetChild(0).localScale * 10);
                    }
                }

                else
                {
                    ServerAddPlane(s,
                        UnityARAnchorManager.Instance.planeAnchorMap[s].gameObject.transform.GetChild(0).position,
                        UnityARAnchorManager.Instance.planeAnchorMap[s].gameObject.transform.GetChild(0).rotation.eulerAngles.y,
                        UnityARAnchorManager.Instance.planeAnchorMap[s].gameObject.transform.GetChild(0).localScale * 10);
                }
            }
            yield return new WaitForSeconds(1f);
        }
    }

    [Server]
    private void ServerAddPlane(string s, Vector3 pos, float rot, Vector3 scale)
    {
        m_ARPlane.Add(new ARPlane(s, pos, rot, scale));
    }

    [Server]
    private void ServerUpdatePlane(int index, Vector3 pos, float rot, Vector3 scale)
    {
        if (index < m_ARPlane.Count)
        {
            m_ARPlane[index] = new ARPlane(m_ARPlane[index].identifier, pos, rot, scale);
            m_ARPlane.Dirty(index);
        }
    }

    [Server]
    private void ServerRemovePlane(int index)
    {
        m_ARPlane.RemoveAt(index);
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
