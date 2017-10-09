using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.iOS;
using UnityEngine.Networking;


public class NetworkedPlaneManager : NetworkBehaviour
{

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

        public void Update(Vector3 position, float rotation, Vector3 scale)
        {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }
    }

    public class ARPlaneSync : SyncListStruct<ARPlane> { }

    ARPlaneSync m_ARPlane = new ARPlaneSync();
    #endregion

    [SerializeField]
    List<GameObject> localPlanes;
    int prevListCount = 0;

    enum GamePhase
    {
        Scanning = 0,
        Playing = 1
    }

    [SyncVar]
    int currGamePhase = (int)GamePhase.Scanning;

    public GameObject planePrefab;

    private Player player;
    private float floorPos;


    public float FloorPos { get { return floorPos; } }

    // Use this for initialization
    void Start()
    {
        player = GetComponent<Player>();
        if (player.PlayerType == PlayerType.AR)
        {
            StartCoroutine("UpdateLocalPlanes");
            localPlanes = new List<GameObject>();
            if (isServer)
            {
                StartCoroutine("UpdateARPlanes");
            }

        }
    }

    private void OnDestroy()
    {
        for (int i = 0; i < localPlanes.Count; i++)
        {
            Destroy(localPlanes[i]);
        }
        localPlanes.Clear();
    }

    private void Update()
    {
        if (isServer)
        {
            if (Input.GetKeyDown(KeyCode.D))
                ServerAddPlane("dfdf",
                    new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)),
                    Random.Range(0f, 360f),
                    new Vector3(Random.Range(3f, 6f), Random.Range(3f, 6f), Random.Range(3f, 6f)));
        }
    }

    IEnumerator UpdateLocalPlanes()
    {
        //only updates if local player
        if (!isLocalPlayer)
            yield return null;

        //endless loop
        for (; ; )
        {
            //add a plane
            if (prevListCount < m_ARPlane.Count)
            {
                GameObject obj = Instantiate(planePrefab);
                localPlanes.Add(obj);
            }

            //destory a plane
            else if (prevListCount > m_ARPlane.Count)
            {
                Destroy(localPlanes[prevListCount - 1]);
                localPlanes.RemoveAt(prevListCount - 1);
            }

            //update all the planes
            for (int i = 0; i < localPlanes.Count; i++)
            {
                //check to make sure plane exists
                if (i < m_ARPlane.Count)
                {
                    localPlanes[i].GetComponent<LocalPlane>().UpdatePos(m_ARPlane[i].position,
                        m_ARPlane[i].rotation,
                        m_ARPlane[i].scale);

                    float yPos = m_ARPlane[i].position.y;

                    floorPos = yPos < floorPos ? yPos : floorPos;
                }
                else
                    break;
            }

            prevListCount = localPlanes.Count;
            yield return new WaitForSeconds(.1f);
        }
    }

    #region Server Functions

#if UNITY_IOS
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
                        ServerRemovePlane(i);
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
						ServerUpdatePlane (index, 						
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
#endif

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
            m_ARPlane[index].Update(pos, rot, scale);
            string identifier = m_ARPlane[index].identifier;
            m_ARPlane[index] = new ARPlane(identifier, pos, rot, scale);
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
