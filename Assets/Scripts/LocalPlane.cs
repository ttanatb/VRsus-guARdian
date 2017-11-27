using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Helper class for updating local plane transforms
/// </summary>
/// 
[ExecuteInEditMode]
public class LocalPlane : MonoBehaviour
{
    private const float HEIGHT = 1f;

    private Vector2 scale = Vector2.one;
    private Material m;

    public void Start()
    {
#if UNITY_IOS
        GetComponent<Renderer>().enabled = false;  
#else
        m = GetComponent<Renderer>().material;
#endif
    }

#if !UNITY_IOS
    private void Update()
    {
        scale = new Vector2(transform.localScale.x, transform.localScale.z);
        if (!m)
            m = GetComponent<Renderer>().material;
        m.SetTextureScale("_MainTex", scale / 0.75f);
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
