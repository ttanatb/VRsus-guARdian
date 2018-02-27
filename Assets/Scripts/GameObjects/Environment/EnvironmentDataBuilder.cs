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
                envObj.AddCapCols(cCol, obj.transform.localScale.x, Vector3.zero);

                BoxCollider[] bCol = obj.GetComponents<BoxCollider>();
                envObj.SetBoxCols(bCol, obj.transform.localScale.x, Vector3.zero);

                int childCount = obj.transform.childCount;
                Debug.Log(obj.name + " has " + childCount + " children");
                for (int j = 0; j < childCount; j++)
                {
                    Transform child = obj.transform.GetChild(j);
                    Debug.Log("Working with " + child.name);
                    MeshRenderer mRenderer = child.GetComponent<MeshRenderer>();
                    if (mRenderer)
                    {
                        Debug.Log("Getting mesh/mat from " + child.name);
                        envObj.AddMeshMat(child.GetComponent<MeshFilter>().sharedMesh,
                            mRenderer.sharedMaterials,
                            child.localScale.x * obj.transform.localScale.x, child.localPosition);
                    }
                    else
                    {
                        Debug.Log(child.name + " has no mesh/mat");
                    }

                    if (child.GetComponent<Collider>())
                    {
                        envObj.AddCapCols(child.GetComponents<CapsuleCollider>(), child.localScale.x, child.localPosition);
                        envObj.SetBoxCols(child.GetComponents<BoxCollider>(), child.localScale.x, child.localPosition);
                    }
                }

                MeshRenderer objMeshRenderer = obj.GetComponent<MeshRenderer>();
                if (objMeshRenderer)
                {
                    envObj.AddMeshMat(obj.GetComponent<MeshFilter>().sharedMesh, objMeshRenderer.sharedMaterials, obj.transform.localScale.x, transform.position);
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

        Debug.Log("Done building Test.asset!");
    }
}