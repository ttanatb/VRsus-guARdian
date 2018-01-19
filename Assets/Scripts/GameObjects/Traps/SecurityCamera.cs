using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Script that manges the functionality of a security camera
/// 
/// Author: Tanat Boozayaangool
/// </summary>
public class SecurityCamera : TrapDefense
{
    #region Fields
    [Tooltip("The Render Textures for the cameras & screens. The count should reflect how many camera screens there are.")]
    public RenderTexture[] renderTextures;

    [Tooltip("The renderer for the 'Area of Effect'")]
    public Renderer areaOfEffectRenderer;

    private static int count;
    private static Transform screenParentObj;

    public override string TrapName { get { return "Security Camera"; } }
    #endregion

    #region Init & Destruction
    private void Start()
    {
        if (isServer) return;

        //Disable components for clients that aren't hosts
        GetComponent<Renderer>().enabled = false;
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.enabled = false;

        foreach (Collider c in GetComponents<Collider>())
        {
            if (!c.isTrigger)
                c.enabled = false;
        }
    }

    public override void OnStartServer()
    {
        //initializes static reference
        if (screenParentObj == null)
        {
            if (!Camera.main)
                Debug.LogError("No main camera");

            //Finds the Security Screen Parent Obj
            for (int i = 0; i < Camera.main.transform.childCount; i++)
            {
                Transform child = Camera.main.transform.GetChild(i);
                if (child.name == "SecurityScreens")
                {
                    screenParentObj = child;
                    break;
                }
            }
        }

        //does set-up on the Camera component
        Camera cam = GetComponentInChildren<Camera>();
        GetComponentInChildren<Camera>().targetTexture = renderTextures[count];
        cam.enabled = true;

        if (count < renderTextures.Length)
            count++;

        //enables the security screen
        if (screenParentObj)
        {
            GameObject screen = screenParentObj.GetChild(count - 1).gameObject;
            screen.SetActive(true);
            screen.GetComponent<SecurityScreen>().associatedCamera = this;
        }
        else
        {
            Debug.LogError("No security screen");
        }
    }

    /// <summary>
    /// Destroys/Disables and static references
    /// </summary>
    public override void OnNetworkDestroy()
    {
        if (!isServer) return;

        if (count > 0)
        {
            for (int i = 0; i < screenParentObj.childCount; i++)
                screenParentObj.GetChild(i).gameObject.SetActive(false);

            count = 0;
        }
    }
    #endregion

    #region Life Cycle
    /// <summary>
    /// Toggles renderer to reflect being selected or not
    /// </summary>
    public override void ToggleSelected()
    {
        base.ToggleSelected();
        areaOfEffectRenderer.enabled = selected;
    }
    #endregion
}
