using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SlowTrap : TrapDefense
{
    [SyncVar]
    bool isActive = false;
    private Renderer areaBox;

    public override string TrapName
    {
        get
        {
            return "Slow Trap";
        }
    }

    private void Start()
    {
        areaBox = transform.GetChild(0).GetComponent<Renderer>();
#if !UNITY_IOS
        GetComponent<Renderer>().enabled = false;
        foreach(Collider c in GetComponents<Collider>())
        {
            if (!c.isTrigger)
            {
                c.enabled = false;
            }
        }
#endif
    }

    public override void ToggleSelected()
    {
        base.ToggleSelected();

        areaBox.enabled = selected;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isServer) return;

        if (collision.gameObject.tag == "Player")
        {
            Debug.Log("Player enterred trigger area");

            if (!isActive)
                RpcTriggerTrap();

            if (isActive)
            {
                collision.gameObject.GetComponent<Movement>().RpcSlow();
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (!isServer) return;

        if (collision.gameObject.tag == "Player" && isActive)
        {
            collision.gameObject.GetComponent<Movement>().RpcUnSlow();
        }
    }

    [ClientRpc]
    private void RpcTriggerTrap()
    {
        if (isLocalPlayer) return;

        isActive = true;
        areaBox.enabled = true;
    }
}
