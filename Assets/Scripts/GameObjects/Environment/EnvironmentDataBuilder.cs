using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class EnvironmentDataBuilder : MonoBehaviour
{
    public EnvironmentData decorList;

    // Use this for initialization
    void Start()
    {
        decorList = ScriptableObject.CreateInstance(typeof(EnvironmentData)) as EnvironmentData;
        List<EnvironmentObjectData> decors = new List<EnvironmentObjectData>();
        List<EnvironmentObjectData> structures = new List<EnvironmentObjectData>();
        List<EnvironmentObjectData> landMarks = new List<EnvironmentObjectData>();
        GameObject[] objs = FindObjectsOfType<GameObject>();

        for (int i = 0; i < objs.Length; i++)
        {
            GameObject obj = objs[i];
            if (obj.transform.parent != null) continue;

            if (obj.name.Substring(0, 3) == "en_")
            {
                Debug.Log("Working on " + obj.name);
                EnvironmentObjectData envObj = new EnvironmentObjectData(0); // Mathf.Max(b.size.x, b.size.z) * scale);

                CapsuleCollider[] cCol = obj.GetComponents<CapsuleCollider>();
                envObj.AddCapCols(cCol);

                BoxCollider[] bCol = obj.GetComponents<BoxCollider>();
                envObj.SetBoxCols(bCol);

                int childCount = obj.transform.childCount;
                for (int j = 0; j < childCount; j++)
                {
                    Transform child = obj.transform.GetChild(j);
                    if (!child.GetComponent<MeshRenderer>()) continue;

                    envObj.AddMeshMat(child.GetComponent<MeshFilter>().sharedMesh,
                        child.GetComponent<MeshRenderer>().sharedMaterials,
                        child.localScale.x, child.localPosition);
                }

                envObj.CalcRadius();
                if (envObj.boxColDatas.Length == 0 && envObj.capColDatas.Length == 0)
                {
                    decors.Add(envObj);
                }
                else if (!envObj.isLandMark)
                {
                    structures.Add(envObj);
                }
                else
                {
                    landMarks.Add(envObj);
                }
            }

            decorList.decorDataList = decors.ToArray();
            decorList.structureDataList = structures.ToArray();
            decorList.landMarkDataList = landMarks.ToArray();
        }
#if UNITY_EDITOR
        AssetDatabase.CreateAsset(decorList, "Assets/EnvironmentAssetData/Test.asset");
        AssetDatabase.SaveAssets();
#endif
    }
}