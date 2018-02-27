using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdgeDistTest : MonoBehaviour
{
    public float radius;
    public LocalPlane plane;
    private bool tooClose = false;
    private List<Vector3> planeList;

    // Update is called once per frame
    void Update()
    {
        if (!plane) {
            plane = FindObjectOfType<LocalPlane>();
            if (!plane) return;
        }
        planeList = Utility.CreateVerticesFromPlane(plane.gameObject);
        tooClose = Utility.CheckIfTooCloseToEdge(planeList, transform.position, radius);
    }

    private void OnDrawGizmos()
    {
    }
}
