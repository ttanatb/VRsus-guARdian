using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalPlane : MonoBehaviour
{

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
        transform.position = center;
        extents.y = 0.01f;
        transform.localScale = extents;
    }
}
