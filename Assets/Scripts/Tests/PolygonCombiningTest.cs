using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolygonCombiningTest : MonoBehaviour
{
    private List<List<Vector3>> allPlanes;

    [SerializeField]
    List<Vector3> vertices = new List<Vector3>();

    EnvironmentCreation terrainBuilder;

    private void Start()
    {
        terrainBuilder = FindObjectOfType<EnvironmentCreation>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            allPlanes = LocalObjectBuilder.Instance.GetSortedPlanes();
            vertices = Utility.CombinePolygons(allPlanes[0], allPlanes[1], 0.2f);
            for (int i = 2; i < allPlanes.Count; i++)
            {
                vertices = Utility.CombinePolygons(vertices, allPlanes[i], 0.2f);
            }

            for (int i = 0; i < vertices.Count; i++)
            {
                Vector3 vert = vertices[i];
                vert.y = 0f;
                vertices[i] = vert;
            }

            terrainBuilder.boundary = vertices;
            terrainBuilder.CreateTerrain();

            LocalPlane[] planes = FindObjectsOfType<LocalPlane>();
            foreach(LocalPlane plane in planes)
            {
                plane.GetComponent<MeshRenderer>().material = terrainBuilder.GetComponent<MeshRenderer>().material;
            }

        }
    }

    private void OnDrawGizmos()
    {
        Color c = Color.white;
        for (int i = 0; i < vertices.Count; i++)
        {
            c = Color.blue * ((float)i / vertices.Count);
            c.a = 0.8f;
            Gizmos.color = c;
            Gizmos.DrawSphere(vertices[i], 0.1f);
        }
    }
}
