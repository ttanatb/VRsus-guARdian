using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolygonCombiningTest : MonoBehaviour
{
    private LocalPlane[] allPlanes;
    public GameObject plane1;
    public GameObject plane2;

    [SerializeField]
    List<Vector3> vertices = new List<Vector3>();
    List<Vector3> vertices1 = new List<Vector3>();
    List<Vector3> vertices2 = new List<Vector3>();

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            //if (plane1)
            //    plane1.GetComponent<Renderer>().enabled = false;

            //if (plane2)
            //    plane2.GetComponent<Renderer>().enabled = false;


            allPlanes = FindObjectsOfType<LocalPlane>();
            int rndmIndex = Random.Range(0, allPlanes.Length);
            int rndmIndex2 = GetNonRepeatingNum(allPlanes.Length, rndmIndex);
            plane1 = allPlanes[rndmIndex].gameObject;
            plane2 = allPlanes[rndmIndex2].gameObject;

            vertices1 = Utility.CreateVerticesFromPlane(plane1);
            vertices2 = Utility.CreateVerticesFromPlane(plane2);
            vertices = Utility.CombinePolygons(vertices1, vertices2, 0.2f);
            for (int i = 0; i < vertices1.Count; i++)
            {
                Vector3 vert = vertices1[i];
                vert.y = 3f;
                vertices1[i] = vert;

                vert = vertices2[i];
                vert.y = 3f;
                vertices2[i] = vert;
            }

            for (int i = 0; i < vertices.Count; i++)
            {
                Vector3 vert = vertices[i];
                vert.y = 3f;
                vertices[i] = vert;
            }

            //plane1.GetComponent<Renderer>().enabled = false;
            //plane2.GetComponent<Renderer>().enabled = false;
        }
    }

    private void OnDrawGizmos()
    {
        Color c = Color.white;
        for (int i = 0; i < vertices1.Count; i++)
        {
            c = Color.white * ((float)i / vertices1.Count);
            c.a = 0.75f;
            Gizmos.color = c;
            //Gizmos.DrawSphere(vertices1[i], 0.2f);
            //Gizmos.DrawSphere(vertices2[i], 0.2f);
        }

        for (int i = 0; i < vertices.Count; i++)
        {
            c = Color.blue * ((float)i / vertices.Count);
            c.a = 0.8f;
            Gizmos.color = c;
            Gizmos.DrawSphere(vertices[i], 0.1f);
        }

        Color coo = Color.red;
        coo.a = 0.2f;
        Gizmos.color = coo;
        if (plane1)
        Gizmos.DrawWireMesh(plane1.GetComponent<MeshFilter>().mesh, plane1.transform.position, plane1.transform.rotation, plane1.transform.localScale);

        if (plane2)
        Gizmos.DrawWireMesh(plane2.GetComponent<MeshFilter>().mesh, plane2.transform.position, plane2.transform.rotation, plane2.transform.localScale);
    }

    private int GetNonRepeatingNum(int count, int firstNum)
    {
        int index = firstNum;
        while (index == firstNum)
        {
            index = Random.Range(0, count);
        }

        return index;
    }
}
