#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(EnvDecorPropTest))]
public class EnvDecorPropTestEditor : Editor {
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EnvDecorPropTest myScript = (EnvDecorPropTest)target;
        if (GUILayout.Button("Spawn Random Obj"))
        {
            myScript.SpawnRandom();
        }

        myScript = (EnvDecorPropTest)target;
        if (GUILayout.Button("Delete Prop Objs"))
        {
            myScript.DeleteOldProps();
        }
    }
}
#endif