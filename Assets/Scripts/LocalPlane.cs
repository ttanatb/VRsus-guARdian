using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Helper class for updating local plane transforms
/// </summary>
public class LocalPlane : MonoBehaviour
{
    private const float HEIGHT = 100f;

#if UNITY_IOS
    public void Start()
    {
        GetComponent<Renderer>().enabled = false;  
    }
#endif

    public void UpdatePos(Vector3 position, float rotation, Vector3 scale)
    {
        transform.position = position;

        scale.y = HEIGHT;
        transform.localScale = scale;

        transform.Translate(HEIGHT / 2 * Vector3.down);

        Vector3 euler = transform.rotation.eulerAngles;
        euler.y = rotation;
        transform.rotation = Quaternion.Euler(euler);
    }
}
