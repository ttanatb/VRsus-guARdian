using UnityEngine;
using UnityEngine.VR;
using System.Collections.Generic;

public class VRInput : MonoBehaviour
{
    //Example of toggling VRSettings
    private void Update()
    {
        //If V is pressed, toggle VRSettings.enabled
        if (Input.GetKeyDown(KeyCode.V))
        {
            UnityEngine.XR.XRSettings.enabled = !UnityEngine.XR.XRSettings.enabled;
            Debug.Log("Changed VRSettings.enabled to:" + UnityEngine.XR.XRSettings.enabled);
        }


    }
}