using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PraticeArea : MonoBehaviour
{

    public Transform spawnPos;
    public GameObject[] planes;
    private EnvironmentCreation env;
    // Use this for initialization
    void Start()
    {
        List<Vector3> list = Utility.CombinePolygons(Utility.CreateVerticesFromPlane(planes[0]),
            Utility.CreateVerticesFromPlane(planes[1]), 0.005f);
        for (int i = 2; i < planes.Length; i++)
        {
            list = Utility.CombinePolygons(list, Utility.CreateVerticesFromPlane(planes[i]), 0.005f);
        }

        for (int i = 0; i < list.Count; i++)
        {
            Vector3 vec = list[i];
            vec.y = 0f;// transform.position.y;
            list[i] = vec;
        }

        env = GetComponent<EnvironmentCreation>();
        env.boundary = list;
        env.CreateTerrain();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
