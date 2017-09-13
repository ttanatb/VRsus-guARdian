using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.iOS;
using UnityEngine.Networking;


public class NetworkedPlaneManager : NetworkBehaviour {

	public struct ARPlane 
	{
		public string identifier;
		public Vector3 center;
		public Vector3 extent;

		public ARPlane(string identifier, Vector3 center, Vector3 extent)
		{
			this.identifier = identifier;
			this.center = center;
			this.extent = extent;
		}

		public void Update(Vector3 center, Vector3 extent)
		{
			this.center = center;
			this.extent = extent;
		}
	}

	public class ARPlaneSync : SyncListStruct<ARPlane> { }
	int prevListCount = 0;

	ARPlaneSync m_ARPlane = new ARPlaneSync();
	List<GameObject> localPlanes;
	public GameObject planePrefab;

	void ARPlaneChanged(SyncListStruct<ARPlane>.Operation op, int itemIndex) 
	{

		if (!isServer)
			return;

//		if (op == SyncList<ARPlane>.Operation.OP_ADD) 
//		{
//			GameObject obj = Instantiate (planePrefab);
//			obj.GetComponent<LocalPlane> ().UpdatePos (m_ARPlane [itemIndex].center, m_ARPlane [itemIndex].extent);
//			localPlanes.Add (obj);
//		} 
//		else if (op == SyncList<ARPlane>.Operation.OP_REMOVEAT) 
//		{
//			Destroy (localPlanes [itemIndex]);
//			localPlanes.RemoveAt (itemIndex);
//		}

		Debug.Log ("AR Plane changed: " + op);
	}

	// Use this for initialization
	void Start ()
	{
		Debug.Log ("Started! isServer: " + isServer + " isLocalPlayer: " + isLocalPlayer);
		if (isServer || isLocalPlayer) {
			m_ARPlane.Callback = ARPlaneChanged;
			localPlanes = new List<GameObject> ();
			#if UNITY_IOS
			StartCoroutine ("UpdateARPlanes");
			#endif
			
			StartCoroutine ("UpdateLocalPlanes");
		}
	}

	void Update ()
	{
		//Debug.Log (UnityARAnchorManager.Instance.planeAnchorMap.Count);
		if (Input.GetKeyDown (KeyCode.D)) {
			//UnityARAnchorManager.Instance.planeAnchorMap.Add ("REEE" + count, new ARPlaneAnchorGameObject ());
		}
	}

	IEnumerator UpdateLocalPlanes()
	{
		if (!isLocalPlayer)
			yield return null;
		for (;;) {
			if (prevListCount < m_ARPlane.Count) {
				GameObject obj = Instantiate (planePrefab);
				localPlanes.Add (obj);
			} else if (prevListCount > m_ARPlane.Count) {
				Destroy (localPlanes [prevListCount - 1]);
				localPlanes.RemoveAt (prevListCount - 1);
			}

			for (int i = 0; i < localPlanes.Count; i++) {
				if (i < m_ARPlane.Count)
					localPlanes [0].GetComponent<LocalPlane> ().UpdatePos (m_ARPlane [0].center, m_ARPlane [0].extent);
				else
					break;
			}

			prevListCount = localPlanes.Count;
			yield return new WaitForSeconds (.1f);
		}
	}

	#if UNITY_IOS
	[Server]
	IEnumerator UpdateARPlanes()
	{
		if (!isServer)
			yield return null;
		for(;;) 
		{
			if (m_ARPlane.Count > UnityARAnchorManager.Instance.planeAnchorMap.Count) {
				for (int i = 0; i < m_ARPlane.Count; i++) {
					if (!UnityARAnchorManager.Instance.planeAnchorMap.ContainsKey (m_ARPlane [i].identifier)) {
						CmdRemovePlane(i);
						break;
							}
						}
			}
			foreach (string s in UnityARAnchorManager.Instance.planeAnchorMap.Keys) 
			{
				if (CheckIfContains (s)) {
					int index = GetIndexOf (s);
					if (index != -1) {
						CmdUpdatePlane(index, UnityARAnchorManager.Instance.planeAnchorMap [s].planeAnchor.center, UnityARAnchorManager.Instance.planeAnchorMap [s].planeAnchor.extent);
					}
				} else 
				{
					CmdAddPlane(s, UnityARAnchorManager.Instance.planeAnchorMap [s].planeAnchor.center, UnityARAnchorManager.Instance.planeAnchorMap [s].planeAnchor.extent);
				}
			}
			yield return new WaitForSeconds (.1f);
		}
	}

	#endif

	[Command]
	private void CmdAddPlane(string s, Vector3 center, Vector3 extents)
	{
		m_ARPlane.Add(new ARPlane(s, center, extents));
	}

	[Command]
	private void CmdUpdatePlane(int index, Vector3 center, Vector3 extents)
	{
		m_ARPlane [index].Update (center, extents);
		m_ARPlane.Dirty (index);
	}

	[Command]
	private void CmdRemovePlane(int index)
	{
		m_ARPlane.RemoveAt (index);
	}

	private bool CheckIfContains(string identifier)
	{
		for (int i = 0; i < m_ARPlane.Count; i++) 
		{
			if (m_ARPlane [i].identifier == identifier) 
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
			if (m_ARPlane [i].identifier == identifier) 
			{
				return i;
			}
		}

		return -1;
	}
}
