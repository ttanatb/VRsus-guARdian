using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SecurityCamera : TrapDefense
{
    public RenderTexture[] renderTextures;

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
            securityScreens.GetChild(count - 1).gameObject.SetActive(true);
            return;
        }
    }

    public override void RpcDisable()
    {
        base.RpcDisable();
    }
}
