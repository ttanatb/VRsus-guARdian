using UnityEngine;
using System.Collections;
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
            myScript.CreateTerrain();
        }
    }
}
