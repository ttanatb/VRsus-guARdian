using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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
        List<EnvironmentObjectData> landMarks = new List<EnvironmentObjectData>();
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

        foreach (GameObject obj in objs)
        {
            //Skip objects that aren't props and aren't the parent object
            if (obj.transform.parent != null) { continue; }
            if (!obj.name.StartsWith("en_")) { continue; }

            string name = obj.name.Split('_')[1];
            Debug.Log("Working on: " + name);

            //Create a new environment object
            EnvironmentObjectData envObj = new EnvironmentObjectData(0);

            //Get the information about the children
            Debug.Log(name + " has " + obj.transform.childCount + " children");

            //Loop through all of the child objects
            foreach (Transform child in obj.transform)
            {
                string childName = child.name.Split('_')[1];
                Debug.Log("Working with: " + childName);

                //Get the child mesh renderer
                MeshRenderer mRenderer = child.GetComponent<MeshRenderer>();

                //If there is no mesh renderer go to the next child
                if (mRenderer)
                {
                    Debug.Log("Getting mesh/mat from: " + childName);

                    //Add the mesh renderer
                    envObj.AddMeshMat(child.GetComponent<MeshFilter>().sharedMesh,
                        mRenderer.sharedMaterials,
                        child.localScale.x * obj.transform.localScale.x,
                        child.localPosition,
                        child.localRotation);
                }
                else
                {
                    Debug.Log(childName + " has no mesh/mat");
                }

                //If the child has colliders add them to the collider lists
                if (child.GetComponent<Collider>())
                {
                    envObj.AddCapCols(child.GetComponents<CapsuleCollider>(), child.localScale.x, child.localPosition);
                    envObj.AddBoxCols(child.GetComponents<BoxCollider>(), child.localScale.x, child.localPosition);
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

            Debug.Log("Created new Env Asset named: " + name);

            //Create and save the asset
            AssetDatabase.CreateAsset(decorList, "Assets/EnvironmentAssetData/" + name + ".asset");
            AssetDatabase.SaveAssets();
        }

        Debug.Log("Done building!");
    }
}