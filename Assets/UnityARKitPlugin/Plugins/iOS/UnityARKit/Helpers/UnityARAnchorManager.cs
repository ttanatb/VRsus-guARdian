using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.XR.iOS
{
	public class UnityARAnchorManager  : Singleton<UnityARAnchorManager>
	{


		private Dictionary<string, ARPlaneAnchorGameObject> planeAnchorMap;

        public Dictionary<string, ARPlaneAnchorGameObject> PlaneAnchorMap {  get { return planeAnchorMap; } }

        public UnityARAnchorManager ()
		{
			planeAnchorMap = new Dictionary<string,ARPlaneAnchorGameObject> ();
			UnityARSessionNativeInterface.ARAnchorAddedEvent += AddAnchor;
			UnityARSessionNativeInterface.ARAnchorUpdatedEvent += UpdateAnchor;
			UnityARSessionNativeInterface.ARAnchorRemovedEvent += RemoveAnchor;

            instance = this;
		}

        public void TogglePlaneMaterial()
        {

        }


		public void AddAnchor(ARPlaneAnchor arPlaneAnchor)
		{
			GameObject go = UnityARUtility.CreatePlaneInScene (arPlaneAnchor);
			go.AddComponent<DontDestroyOnLoad> ();  //this is so these GOs persist across scene loads
			ARPlaneAnchorGameObject arpag = new ARPlaneAnchorGameObject ();
			arpag.planeAnchor = arPlaneAnchor;
			arpag.gameObject = go;

            DebugPlaneTiler planeTiler = go.GetComponentInChildren<DebugPlaneTiler>();
            if (planeTiler)
                planeTiler.AddToList();

			planeAnchorMap.Add (arPlaneAnchor.identifier, arpag);
		}

		public void RemoveAnchor(ARPlaneAnchor arPlaneAnchor)
		{
            if (planeAnchorMap.ContainsKey(arPlaneAnchor.identifier))
            {
                ARPlaneAnchorGameObject arpag = planeAnchorMap[arPlaneAnchor.identifier];
                DebugPlaneTiler planeTiler = arpag.gameObject.GetComponentInChildren<DebugPlaneTiler>();
                if (planeTiler)
                    planeTiler.RemoveFromList();

                GameObject.Destroy(arpag.gameObject);
                planeAnchorMap.Remove(arPlaneAnchor.identifier);
            }
		}

		public void UpdateAnchor(ARPlaneAnchor arPlaneAnchor)
		{
			if (planeAnchorMap.ContainsKey (arPlaneAnchor.identifier)) {
				ARPlaneAnchorGameObject arpag = planeAnchorMap [arPlaneAnchor.identifier];
				UnityARUtility.UpdatePlaneWithAnchorTransform (arpag.gameObject, arPlaneAnchor);
				arpag.planeAnchor = arPlaneAnchor;
				planeAnchorMap [arPlaneAnchor.identifier] = arpag;

                DebugPlaneTiler planeTiler = arpag.gameObject.GetComponentInChildren<DebugPlaneTiler>();
                if (planeTiler)
                    planeTiler.UpdateWithinList();
            }
		}

        public void UnsubscribeEvents()
        {
            UnityARSessionNativeInterface.ARAnchorAddedEvent -= AddAnchor;
            UnityARSessionNativeInterface.ARAnchorUpdatedEvent -= UpdateAnchor;
            UnityARSessionNativeInterface.ARAnchorRemovedEvent -= RemoveAnchor;
        }

        public void Destroy()
        {
            foreach (ARPlaneAnchorGameObject arpag in GetCurrentPlaneAnchors()) {
                GameObject.Destroy (arpag.gameObject);
            }

            planeAnchorMap.Clear ();
            UnsubscribeEvents();
        }

		public List<ARPlaneAnchorGameObject> GetCurrentPlaneAnchors()
		{
			return planeAnchorMap.Values.ToList ();
		}
	}
}

