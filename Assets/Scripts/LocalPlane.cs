using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalPlane : MonoBehaviour
{
	[SerializeField]
	float timeSinceLastUpdate = 0;

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdatePos(Vector3 center, Vector3 extents)
    {
		timeSinceLastUpdate = 0;

        transform.position = center;
        extents.y = 0.01f;
        transform.localScale = extents;
    }
}
