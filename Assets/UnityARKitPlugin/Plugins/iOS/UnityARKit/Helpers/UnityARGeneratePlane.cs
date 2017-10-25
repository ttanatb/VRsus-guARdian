using System;
using System.Collections.Generic;

namespace UnityEngine.XR.iOS
{
    public class UnityARGeneratePlane : MonoBehaviour
    {
        public GameObject planePrefab;
#if UNITY_IOS
        private UnityARAnchorManager unityARAnchorManager;

        // Use this for initialization
        void Start()
        {
            unityARAnchorManager = new UnityARAnchorManager();
            UnityARUtility.InitializePlanePrefab(planePrefab);
        }

        void OnDestroy()
        {
            unityARAnchorManager.Destroy();
        }

		int prevCount = 0;
		void Update() {
			List<ARPlaneAnchorGameObject> arpags = unityARAnchorManager.GetCurrentPlaneAnchors();

			if (arpags.Count != prevCount) {
				//Debug.Log ("Added plane");
			}

			prevCount = arpags.Count;
		}
#endif
    }
}
