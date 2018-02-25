using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvDecorPropTest : MonoBehaviour
{
    public EnvironmentData decorListObj;
    public GameObject envObjPrefab;

    public void SpawnRandom()
    {
        int count = decorListObj.decorDataList.Count;
        float x = -2f;
        for (int i = 0; i < count; i++)
        {
            EnvironmentObjectData data = decorListObj.decorDataList[i];

            x += data.radius;
            EnvironmentObject obj = Instantiate(envObjPrefab,
                new Vector3(x, 0f, 0f),
                Quaternion.identity).GetComponent<EnvironmentObject>();
            x += data.radius;

            obj.Initialize(data);
        }


        count = decorListObj.structuresDataList.Count;
        for (int i = 0; i < count; i++)
        {
            EnvironmentObjectData data = decorListObj.structuresDataList[i];

            x += data.radius;
            EnvironmentObject obj = Instantiate(envObjPrefab,
                new Vector3(x, 0f, 0f),
                Quaternion.identity).GetComponent<EnvironmentObject>();
            x += data.radius;

            obj.Initialize(data);
        }

        count = decorListObj.landMarkDataList.Count;
        for (int i = 0; i < count; i++)
        {
            EnvironmentObjectData data = decorListObj.landMarkDataList[i];

            x += data.radius;
            EnvironmentObject obj = Instantiate(envObjPrefab,
                new Vector3(x, 0f, 0f),
                Quaternion.identity).GetComponent<EnvironmentObject>();
            x += data.radius;

            obj.Initialize(data);
        }
    }
}
