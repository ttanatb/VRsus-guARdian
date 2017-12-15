using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SecurityCamera : TrapDefense
{
    public RenderTexture[] renderTextures;
    public Renderer areaOfEffectRenderer;

    static int count;
    static Transform securityScreens;

    public override string TrapName
    {
        get
        {
            return "Security Camera";
        }
    }

    private void Start()
    {
        if (isServer)
        {
            Camera cam = GetComponentInChildren<Camera>();
            GetComponentInChildren<Camera>().targetTexture = renderTextures[count];
            cam.enabled = true;

            if (count < renderTextures.Length)
                count++;

            Init();
        }
        else
        {
            foreach (Renderer r in GetComponentsInChildren<Renderer>())
                r.enabled = false;
        }
    }

    public void Init()
    {
        Debug.Log(Camera.main.name);

        if (securityScreens == null)
        {
            if (!Camera.main)
            {
                Debug.Log("No main camera");
            }

            for (int i = 0; i < Camera.main.transform.childCount; i++)
            {
                Transform child = Camera.main.transform.GetChild(i);
                if (child.name == "SecurityScreens")
                    securityScreens = child;
            }
        }

        if (securityScreens)
        {
            GameObject screen = securityScreens.GetChild(count - 1).gameObject;
            screen.SetActive(true);
            screen.GetComponent<SecurityScreen>().associatedCamera = this;
        }
        else
        {
            Debug.LogError("No security screen");
        }
    }

    public override void ToggleSelected()
    {
        base.ToggleSelected();
        //Debug.Log("Toggling this Camera");
        areaOfEffectRenderer.enabled = selected;
    }

    public override void RpcDisable()
    {
        base.RpcDisable();
    }
}
