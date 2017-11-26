using System;
using System.Collections.Generic;

namespace UnityEngine.XR.iOS
{
    public class UnityARGeneratePlane : MonoBehaviour
    {
        public GameObject debugPlanePrefab;
        public GameObject occlusionPlanePrefab;

        private GameObject planePrefab;
        private UnityARAnchorManager unityARAnchorManager;

#if UNITY_IOS
        // Use this for initialization
        void Start()
        {
            planePrefab = debugPlanePrefab;

            unityARAnchorManager = new UnityARAnchorManager();
            UnityARAnchorManager.Instance.SetMaterial(debugPlanePrefab.GetComponentInChildren<Renderer>().material,
                occlusionPlanePrefab.GetComponentInChildren<Renderer>().material);
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
