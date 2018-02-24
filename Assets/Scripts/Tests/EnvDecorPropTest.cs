using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvDecorPropTest : MonoBehaviour
{
    public EnvDecorList decorListObj;
    public GameObject envObjPrefab;

    public void SpawnRandom()
    {
        EnvDecorObj obj = Instantiate(envObjPrefab, 
            new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)), 
            Quaternion.identity).GetComponent<EnvDecorObj>();
        obj.Initialize(decorListObj.decorPrefabList[Random.Range(0, decorListObj.decorPrefabList.Count)]);
    }
}
