using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvDecorPropTest : MonoBehaviour
{
    public EnvDecorList decorListObj;
    public GameObject envObjPrefab;

    public void SpawnRandom()
    {
        int count = decorListObj.propDataList.Count;
        float x = -2f;
        for (int i = 0; i < count; i++)
        {
            DecorEnvObj data = decorListObj.propDataList[i];

            x += data.radius;
            EnvDecorObj obj = Instantiate(envObjPrefab,
                new Vector3(x, 0f, 0f), 
                Quaternion.identity).GetComponent<EnvDecorObj>();
            x += data.radius;

            obj.Initialize(data); // decorListObj.propDataList[Random.Range(0, decorListObj.propDataList.Count)]);
        }
    }
}
