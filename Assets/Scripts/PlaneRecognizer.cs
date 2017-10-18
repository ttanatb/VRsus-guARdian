using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.iOS;

public class PlaneRecognizer : MonoBehaviour
{
    struct ARPlane
    {
        public Vector3 position;
        public float rotation;
        public Vector3 scale;

        public ARPlane(Vector3 position, float rotation, Vector3 scale)
        {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }
    }

    //List<ARPlane> m_ARPlane = new List<ARPlane>();
    Dictionary<string, ARPlane> m_ARPlane = new Dictionary<string, ARPlane>();

    // Use this for initialization
    void Start()
    {
        StartCoroutine("UpdateARPlanes");
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator UpdateARPlanes()
    {
        for (; ; )
        {
            if (m_ARPlane.Count > UnityARAnchorManager.Instance.planeAnchorMap.Count)
            {
                foreach (string key in m_ARPlane.Keys)
                {
                    if (!UnityARAnchorManager.Instance.planeAnchorMap.ContainsKey(key))
                    {
                        RemovePlane(key);
                        break;
                    }

                }
            }

            foreach (string key in UnityARAnchorManager.Instance.planeAnchorMap.Keys)
            {
                if (m_ARPlane.ContainsKey(key))
                {
                    Transform t = UnityARAnchorManager.Instance.planeAnchorMap[key].gameObject.transform.GetChild(0);
                    UpdatePlane(key, t.position, t.rotation.eulerAngles.y, t.localScale * 10);
                }

                else
                {
                    Transform t = UnityARAnchorManager.Instance.planeAnchorMap[key].gameObject.transform.GetChild(0);
                    AddPlane(key, t.position, t.rotation.eulerAngles.y, t.localScale * 10);
                }
            }

            yield return new WaitForSeconds(1f);
        }
    }
    private void AddPlane(string key, Vector3 pos, float rot, Vector3 scale)
    {
        m_ARPlane.Add(key, new ARPlane(pos, rot, scale));
    }

    private void UpdatePlane(string key, Vector3 pos, float rot, Vector3 scale)
    {
        m_ARPlane[key] = new ARPlane(pos, rot, scale);
    }

    private void RemovePlane(string key)
    {
        m_ARPlane.Remove(key);
    }
}
