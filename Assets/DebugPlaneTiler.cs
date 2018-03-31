using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugPlaneTiler : MonoBehaviour {

	private Vector2 scale = Vector2.one;        //x,z scale of tower
	private Material m; 

	// Use this for initialization
	void Start () 
	{
		m = GetComponent<Renderer>().material;
	}
	
	// Update is called once per frame
	void Update () 
	{
		scale.x = transform.localScale.x * transform.parent.localScale.x;
        scale.y = transform.localScale.z * transform.parent.localScale.z;

        if (!m)
            m = GetComponent<Renderer>().material;

        scale = scale / 0.02f;
        m.SetTextureScale("_MainTex", scale);
        m.SetTextureOffset("_MainTex", scale);
	}
}
