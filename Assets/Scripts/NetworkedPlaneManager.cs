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
	ARPlaneSync m_ARPlane = new ARPlaneSync();

	void ARPlaneChanged(SyncListStruct<ARPlane>.Operation op, int itemIndex) 
	{
		Debug.Log ("AR Plane changed: " + op);
	}

	// Use this for initialization
	void Start () {
		m_ARPlane.Callback = ARPlaneChanged;
		#if UNITY_IOS
		StartCoroutine("UpdateARPlanes");
		#endif

	}

	void Update ()
	{
		//Debug.Log (UnityARAnchorManager.Instance.planeAnchorMap.Count);
	}

	#if UNITY_IOS
	IEnumerator UpdateARPlanes()
	{
		for(;;) 
		{
			foreach (string s in UnityARAnchorManager.Instance.planeAnchorMap.Keys) 
			{
				if (CheckIfContains (s)) {
					int index = GetIndexOf (s);
					if (index != -1) {
						m_ARPlane [index].Update (UnityARAnchorManager.Instance.planeAnchorMap [s].planeAnchor.center, UnityARAnchorManager.Instance.planeAnchorMap [s].planeAnchor.extent);
					}
				} else 
				{
					m_ARPlane.Add(new ARPlane(s, UnityARAnchorManager.Instance.planeAnchorMap [s].planeAnchor.center, UnityARAnchorManager.Instance.planeAnchorMap [s].planeAnchor.extent));
				}
			}
			yield return new WaitForSeconds (.1f);
		}
	}
	#endif

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
