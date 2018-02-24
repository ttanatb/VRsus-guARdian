using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EnvPropBuilder : MonoBehaviour
{

    public EnvDecorList decorList;

    // Use this for initialization
    void Start()
    {
        decorList = ScriptableObject.CreateInstance(typeof(EnvDecorList)) as EnvDecorList;
        GameObject[] objs = FindObjectsOfType<GameObject>();
        if (decorList.decorPrefabList == null)
        {
            decorList.decorPrefabList = new List<DecorEnvObj>();
        }

        decorList.decorPrefabList.Clear();
        for (int i = 0; i < objs.Length; i++)
        {
            GameObject obj = objs[i];
            Debug.Log(obj.name);
            if (obj.name.Substring(0, 3) == "en_")
            {
                Mesh mesh = obj.GetComponentInChildren<MeshFilter>().sharedMesh;
                Bounds b = mesh.bounds;
                float scale = obj.transform.GetChild(0).localScale.x;
                decorList.decorPrefabList.Add(new DecorEnvObj(mesh, 
                    obj.GetComponentInChildren<MeshRenderer>().sharedMaterial,
                    Mathf.Max(b.size.x, b.size.z) * scale));
            }
        }

        AssetDatabase.CreateAsset(decorList, "Assets/EnvironmentAssetData/Test.asset");
        AssetDatabase.SaveAssets();
    }
}