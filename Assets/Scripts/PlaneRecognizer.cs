using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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

        public override string ToString()
        {
            return "X: " + scale.x + " Z: " + scale.z;
        }
    }

    //List<ARPlane> m_ARPlane = new List<ARPlane>();
    Dictionary<string, ARPlane> m_ARPlane = new Dictionary<string, ARPlane>();
    List<Text> texts = new List<Text>();

    // Use this for initialization
    void Start()
    {
        StartCoroutine("UpdateARPlanes");
    }

    // Update is called once per frame
    void Update()
    {
        while (texts.Count < m_ARPlane.Count)
            texts.Add(CreateTextUI(texts.Count));

        int counter = 0;
        foreach (string key in m_ARPlane.Keys)
        {
            texts[counter].text = m_ARPlane[key].ToString();
            counter++;
        }
    }

    Text CreateTextUI(int count)
    {
        GameObject obj = new GameObject();
        obj.name = "Text : " + count;
        obj.transform.SetParent(GameObject.Find("Canvas").transform, false);
        RectTransform rect = obj.AddComponent<RectTransform>();

        Vector3 pos = Vector3.zero;
        pos.x = 10;
        pos.y = -10;
        for(int i = 0; i < count; i++)
        {
            pos.y -= 25;
        }

        rect.localPosition = pos;

        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.sizeDelta = new Vector2(160, 15);

        obj.AddComponent<CanvasRenderer>();
        Text text = obj.AddComponent<Text>();
        text.font = Font.CreateDynamicFontFromOSFont("Arial", 15);
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.text = "REE";

        return text;
    }

    IEnumerator UpdateARPlanes()
    {
        for (; ; )
        {
            yield return new WaitForSeconds(1f);

            if (UnityARAnchorManager.Instance == null)
                continue;

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
