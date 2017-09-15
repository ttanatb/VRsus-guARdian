using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyIfVR : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
#if !UNITY_IOS
        Destroy(gameObject);
#endif
    }
}
