using UnityEngine;

/// <summary>
/// Used to destroy ARkit components/objects that isn't required
/// 
/// Author: Tanat Boozayaangool
/// </summary>
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
