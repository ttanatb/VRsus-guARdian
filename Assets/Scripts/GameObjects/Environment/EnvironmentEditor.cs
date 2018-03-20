#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(EnvironmentCreation))]
public class EnvironmentEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EnvironmentCreation myScript = (EnvironmentCreation)target;
        if (GUILayout.Button("Rebuild Mesh"))
        {
            LocalObjectBuilder localObjBuilder = myScript.GetComponent<LocalObjectBuilder>();
            if(localObjBuilder)
            {
                List<List<Vector3>> sortedPlanes = localObjBuilder.GetSortedPlanes();
                float floor = localObjBuilder.FloorPos;// + FindObjectOfType<LocalPlane>().transform.localScale.y / 2f;

                List<Vector3> vertices = new List<Vector3>();
                vertices = Utility.CombinePolygons(sortedPlanes[0], sortedPlanes[1], 0.2f);
                for (int i = 2; i < sortedPlanes.Count; i++)
                {
                    vertices = Utility.CombinePolygons(vertices, sortedPlanes[i], 0.2f);
                }

                for (int i = 0; i < vertices.Count; i++)
                {
                    Vector3 vert = vertices[i];
                    vert.y = floor;
                    vertices[i] = vert;
                }

                myScript.boundary = vertices;
            }
            myScript.CreateTerrain();
        }
    }
}
#endif