using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
public class EnvironmentDataBuilder : MonoBehaviour
{
    public EnvironmentData decorList = null;
    public string saveName = "";

    // Use this for initialization
    void Start()
    {
        //Create lists to store the environment information
        List<EnvironmentObjectData> decors = new List<EnvironmentObjectData>();
        List<EnvironmentObjectData> structures = new List<EnvironmentObjectData>();
        List<EnviromentLandmarkData> landMarks = new List<EnviromentLandmarkData>();
        GameObject[] objs = FindObjectsOfType<GameObject>();

        //Whether data should be overwritten or not
        bool overwriteData = true;

        //Create a new instance of environment data
        if (decorList == null || saveName != "")
        {
            decorList = ScriptableObject.CreateInstance(typeof(EnvironmentData)) as EnvironmentData;
            overwriteData = false;
        }
        else
        {
            //Set all of the array data in the eviroment object
            decorList.decorDataList = null;
            decorList.structureDataList = null;
            decorList.landMarkDataList = null;
        }

        int decorOffset = 0;
        int structureOffset = 0;

        foreach (GameObject obj in objs)
        {
            decorOffset = decors.Count;
            structureOffset = structures.Count;

            //Skip objects that aren't props and aren't the parent object
            if (obj.transform.parent != null) { continue; }
            if (!obj.name.StartsWith("en_") && !obj.name.StartsWith("lm_")) { continue; }

            string name = obj.name.Split('_')[1];
            //Debug.Log("Working on: " + name);

            //Create a new environment object
            EnvironmentObjectData envObj = new EnvironmentObjectData(0);

            //Get the information about the children
            //Debug.Log(name + " has " + obj.transform.childCount + " children");

            //Loop through all of the child objects
            foreach (Transform child in obj.transform)
            {
                EnvironmentObjectData localObj = new EnvironmentObjectData(0);

                string childName = child.name.Split('_')[1];
                //Debug.Log("Working with: " + childName);

                //Get the child mesh renderer
                MeshRenderer mRenderer = child.GetComponent<MeshRenderer>();

                //If there is no mesh renderer go to the next child
                if (mRenderer)
                {
                    //Debug.Log("Getting mesh/mat from: " + childName);

                    //Add the mesh renderer
                    envObj.AddMeshMat(child.GetComponent<MeshFilter>().sharedMesh,
                        mRenderer.sharedMaterials,
                        child.localScale.x * obj.transform.localScale.x,
                        child.localPosition,
                        child.localRotation);

                    localObj.AddMeshMat(child.GetComponent<MeshFilter>().sharedMesh,
                        mRenderer.sharedMaterials,
                        child.localScale.x * obj.transform.localScale.x,
                        child.localPosition,
                        child.localRotation);
                }
                else
                {
                    //Debug.Log(childName + " has no mesh/mat");
                }

                //If the child has colliders add them to the collider lists
                if (child.GetComponent<Collider>())
                {
                    envObj.AddCapCols(child.GetComponents<CapsuleCollider>(), child.localScale.x, child.localPosition);
                    envObj.AddBoxCols(child.GetComponents<BoxCollider>(), child.localScale.x, child.localPosition);

                    localObj.AddCapCols(child.GetComponents<CapsuleCollider>(), child.localScale.x, child.localPosition);
                    localObj.AddBoxCols(child.GetComponents<BoxCollider>(), child.localScale.x, child.localPosition);

                    //Get and save all capsule colliders
                    CapsuleCollider[] lcCol = obj.GetComponents<CapsuleCollider>();
                    localObj.AddCapCols(lcCol, obj.transform.localScale.x, Vector3.zero);

                    //Get and save all box colliders
                    BoxCollider[] lbCol = obj.GetComponents<BoxCollider>();
                    localObj.AddBoxCols(lbCol, obj.transform.localScale.x, Vector3.zero);
                }

                if (obj.name.StartsWith("lm_"))
                {
                    localObj.CalcRadius();

                    if (child.GetComponent<Collider>())
                        structures.Add(localObj);
                    else
                        decors.Add(localObj);
                }
            }

            //Get the mesh renderer of the object
            MeshRenderer objRenderer = obj.GetComponent<MeshRenderer>();

            //Add the mesh renderer
            if (objRenderer)
            {
                envObj.AddMeshMat(obj.GetComponent<MeshFilter>().sharedMesh,
                    objRenderer.sharedMaterials,
                    obj.transform.localScale.x,
                    Vector3.zero,
                    obj.transform.localRotation);
            }

            //Get and save all capsule colliders
            CapsuleCollider[] cCol = obj.GetComponents<CapsuleCollider>();
            envObj.AddCapCols(cCol, obj.transform.localScale.x, Vector3.zero);

            //Get and save all box colliders
            BoxCollider[] bCol = obj.GetComponents<BoxCollider>();
            envObj.AddBoxCols(bCol, obj.transform.localScale.x, Vector3.zero);

            envObj.CalcRadius();

            //Set the type of prop the object is
            if (!obj.name.StartsWith("lm_") && envObj.boxColDatas.Length == 0 && envObj.capColDatas.Length == 0)
            {
                decors.Add(envObj);
            }
            else if (!obj.name.StartsWith("lm_") && !envObj.isLandMark)
            {
                structures.Add(envObj);
            }

            if (obj.name.StartsWith("lm_"))
            {
                EnviromentLandmarkData landMark = new EnviromentLandmarkData(decors.Count - decorList.decorDataList.Length, structures.Count - decorList.structureDataList.Length);

                for (int i = 0; i < decors.Count - decorOffset; i++)
                {
                    landMark.decors[i] = decorOffset + i;
                }

                for (int i = 0; i < structures.Count - structureOffset; i++)
                {
                    landMark.structures[i] = structureOffset + i;
                }

                landMarks.Add(landMark);
            }

            //Set all of the array data in the evironment object
            decorList.decorDataList = decors.ToArray();
            decorList.structureDataList = structures.ToArray();
            decorList.landMarkDataList = landMarks.ToArray();
        }

        //Create a new asset if an asset to overwrite wasn't provided
        if (Application.isEditor && !overwriteData)
        {
            //Set the name of the new environment asset
            string name = (saveName == "") ? Random.Range(0, int.MaxValue).ToString() : saveName;

            //Debug.Log("Created new Env Asset named: " + name);

#if UNITY_EDITOR
            //Create and save the asset
            AssetDatabase.CreateAsset(decorList, "Assets/EnvironmentAssetData/" + name + ".asset");
            AssetDatabase.SaveAssets();
#endif
        }

        //Debug.Log("Done building!");
    }
}