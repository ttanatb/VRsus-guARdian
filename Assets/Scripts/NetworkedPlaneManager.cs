using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.iOS;
using UnityEngine.Networking;


public class NetworkedPlaneManager : NetworkBehaviour
{

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

    int prevListCount = 0;

    [SerializeField]
    List<GameObject> localPlanes;
    public GameObject planePrefab;

    void ARPlaneChanged(SyncListStruct<ARPlane>.Operation op, int itemIndex)
    {
		//Debug.Log ("AR Plane changed: " + op + " at " + itemIndex);
    }

    // Use this for initialization
    void Start()
    {
        //Debug.Log("Started! isServer: " + isServer + " isLocalPlayer: " + isLocalPlayer);
        if (isServer || isLocalPlayer)
        {
            m_ARPlane.Callback = ARPlaneChanged;
            localPlanes = new List<GameObject>();

            if (isServer)
            {
#if UNITY_IOS
			StartCoroutine ("UpdateARPlanes");
#endif
            }
            StartCoroutine("UpdateLocalPlanes");
        }
    }

    void Update()
    {
        //Debug.Log (UnityARAnchorManager.Instance.planeAnchorMap.Count);
        //if (Input.GetKeyDown(KeyCode.D))
        //UnityARAnchorManager.Instance.planeAnchorMap.Add ("REEE" + count, new ARPlaneAnchorGameObject ());
    }

    private void OnDestroy()
    {
        for (int i = 0; i < localPlanes.Count; i++)
        {
            Destroy(localPlanes[i]);
        }
        localPlanes.Clear();
    }

    IEnumerator UpdateLocalPlanes()
    {
        if (!isLocalPlayer)
            yield return null;
        for (; ; )
        {
            if (prevListCount < m_ARPlane.Count)
            {
                GameObject obj = Instantiate(planePrefab);
                localPlanes.Add(obj);
            }
            else if (prevListCount > m_ARPlane.Count)
            {
                Destroy(localPlanes[prevListCount - 1]);
                localPlanes.RemoveAt(prevListCount - 1);
            }

            for (int i = 0; i < localPlanes.Count; i++)
            {
                if (i < m_ARPlane.Count)
                    localPlanes[i].GetComponent<LocalPlane>().UpdatePos(m_ARPlane[i].position,
                        m_ARPlane[i].rotation,
                        m_ARPlane[i].scale);
                else
                    break;
            }

            prevListCount = localPlanes.Count;
            yield return new WaitForSeconds(.1f);
        }
    }

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
                        CmdRemovePlane(i);
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
						CmdUpdatePlane (index, 						
							UnityARAnchorManager.Instance.planeAnchorMap[s].gameObject.transform.GetChild(0).position,
							UnityARAnchorManager.Instance.planeAnchorMap[s].gameObject.transform.GetChild(0).rotation.eulerAngles.y,
							UnityARAnchorManager.Instance.planeAnchorMap[s].gameObject.transform.GetChild(0).localScale * 10);
                    }
                }

                else
                {
					CmdAddPlane(s, 
						UnityARAnchorManager.Instance.planeAnchorMap[s].gameObject.transform.GetChild(0).position,
						UnityARAnchorManager.Instance.planeAnchorMap[s].gameObject.transform.GetChild(0).rotation.eulerAngles.y,
						UnityARAnchorManager.Instance.planeAnchorMap[s].gameObject.transform.GetChild(0).localScale * 10);
                }
            }
            yield return new WaitForSeconds(.3f);
        }
    }
#endif

    [Command]
    private void CmdAddPlane(string s, Vector3 pos, float rot, Vector3 scale)
    {
        m_ARPlane.Add(new ARPlane(s, pos, rot, scale));
    }

    [Command]
    private void CmdUpdatePlane(int index, Vector3 pos, float rot, Vector3 scale)
    {
        if (index < m_ARPlane.Count)
        {
            m_ARPlane[index].Update(pos, rot, scale);
			string identifier = m_ARPlane [index].identifier;
			m_ARPlane [index] = new ARPlane (identifier, pos, rot, scale);
            m_ARPlane.Dirty(index);
        }
    }

    [Command]
    private void CmdRemovePlane(int index)
    {
        m_ARPlane.RemoveAt(index);
    }

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
}
