using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class EnvDecorPropTest : MonoBehaviour
{
    public EnvironmentData decorListObj;
    public GameObject envObjPrefab;

    void Start()
    {
        DeleteOldProps();    
    }

    public void SpawnRandom()
    {
        DeleteOldProps();

        int count = decorListObj.decorDataList.Length;
        float x = -2f;
        for (int i = 0; i < count; i++)
        {
            EnvironmentObjectData data = decorListObj.decorDataList[i];

            x += data.radius;
            EnvironmentObject obj = Instantiate(envObjPrefab,
                new Vector3(x, 0f, 0f),
                Quaternion.identity).GetComponent<EnvironmentObject>();
            x += data.radius;

            obj.Init(0,i);
        }


        count = decorListObj.structureDataList.Length;
        for (int i = 0; i < count; i++)
        {
            EnvironmentObjectData data = decorListObj.structureDataList[i];

            x += data.radius;
            EnvironmentObject obj = Instantiate(envObjPrefab,
                new Vector3(x, 0f, 0f),
                Quaternion.identity).GetComponent<EnvironmentObject>();
            x += data.radius;

            obj.Init(1, i);
        }

        count = decorListObj.landMarkDataList.Length;
        for (int i = 0; i < count; i++)
        {
            EnvironmentObjectData data = decorListObj.landMarkDataList[i];

            x += data.radius;
            EnvironmentObject obj = Instantiate(envObjPrefab,
                new Vector3(x, 0f, 0f),
                Quaternion.identity).GetComponent<EnvironmentObject>();
            x += data.radius;

            obj.Init(2, i);
        }
    }

    public void DeleteOldProps()
    {
        //Get a list of all objects in the scene
        List<GameObject> objs = FindObjectsOfType<GameObject>().ToList();

        //Loop through all of the objects
        foreach (GameObject obj in objs)
        {
            //Destroy all of the objects that are props
            if (obj.name == "_enprop(Clone)")
            {
                //Set the objects to be active
                obj.SetActive(true);

                //Test the running state of the application
                if (EditorApplication.isPlaying)
                {
                    //Destroy objects in play/test mode
                    Destroy(obj);
                }
                else
                {
                    //Destroy objects in edit mode
                    DestroyImmediate(obj);
                }
            } 
        }
    }
}
