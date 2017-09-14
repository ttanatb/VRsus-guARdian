using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalPlane : MonoBehaviour
{
    [SerializeField]
    float timeSinceLastUpdate = 0;

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdatePos(Vector3 position, float rotation, Vector3 scale)
    {
        timeSinceLastUpdate = 0;

        transform.position = position;

        scale.y = 0.01f;
        transform.localScale = scale;

        Vector3 euler = transform.rotation.eulerAngles;
        euler.y = rotation;
        transform.rotation = Quaternion.Euler(euler);
    }
}
