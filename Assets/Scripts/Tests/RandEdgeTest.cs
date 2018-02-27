using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandEdgeTest : MonoBehaviour
{
    Vector3[] positions;
    float[] radii;

    public LocalPlane plane;
    public int count = 200;
    public float rMin = 0.2f;
    public float rMax = 0.5f;
    // Use this for initialization
    void Start()
    {
        positions = new Vector3[0];
        radii = new float[0];
    }

    // Update is called once per frame
    void Update()
    {
        if (!plane)
        {
            plane = FindObjectOfType<LocalPlane>();
            if (!plane) return;
        }

        if (Input.GetKeyDown(KeyCode.T)) GeneratePositions();
    }

    void GeneratePositions()
    {
        positions = new Vector3[count];
        radii = new float[count];

        for (int i = 0; i < count; i++)
        {
            radii[i] = Random.Range(rMin, rMax);
            positions[i] = Utility.GetRandPosInPlaneAndFarFromEdge(Utility.CreateVerticesFromPlane(plane.gameObject), radii[i], 10);
        }
    }

    private void OnDrawGizmos()
    {
        if (positions == null) return;
        for (int i = 0; i < positions.Length; i++)
        {
            Gizmos.DrawSphere(positions[i], radii[i]);
        }
    }
}
