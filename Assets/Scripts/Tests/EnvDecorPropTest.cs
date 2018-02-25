using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvDecorPropTest : MonoBehaviour
{
    public EnvironmentData decorListObj;
    public GameObject envObjPrefab;

    public void SpawnRandom()
    {
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

            obj.RpcInit(0,i);
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

            obj.RpcInit(1, i);
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

            obj.RpcInit(2, i);
        }
    }
}
