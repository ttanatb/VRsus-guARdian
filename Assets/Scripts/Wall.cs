using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour
{
    private Vector2 scale = Vector2.one;
    private Material m;
    // Use this for initialization
    void Start()
    {
        m = GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        scale = new Vector2(transform.localScale.x, transform.localScale.z);
        if (!m)
            m = GetComponent<Renderer>().material;
        m.SetTextureScale("_MainTex", scale / 0.75f);
    }
}
