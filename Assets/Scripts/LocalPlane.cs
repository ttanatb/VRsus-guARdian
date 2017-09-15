using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalPlane : MonoBehaviour
{
    //[SerializeField]
    //float timeSinceLastUpdate = 0;

    public void UpdatePos(Vector3 position, float rotation, Vector3 scale)
    {
        //timeSinceLastUpdate = 0;

        transform.position = position;

        scale.y = 100f;
        transform.localScale = scale;

        transform.Translate(50f * Vector3.down);

        Vector3 euler = transform.rotation.eulerAngles;
        euler.y = rotation;
        transform.rotation = Quaternion.Euler(euler);
    }
}
