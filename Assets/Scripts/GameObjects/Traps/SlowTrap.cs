using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SlowTrap : TrapDefense
{
    public float radius = 5f;
    public MeshRenderer areaRenderer;
    private SphereCollider radiusCollider;
    private bool isActive = false;

    public override string TrapName { get { return "Slow Trap"; } }

    private void Start()
    {
        transform.GetChild(0).localScale *= radius * 2;// transform.localScale.x;
    }

    public override void OnStartServer()
    {
        GetComponent<MeshRenderer>().enabled = true;
        radiusCollider = gameObject.AddComponent<SphereCollider>();
        radiusCollider.radius = radius;
        radiusCollider.isTrigger = true;
        radiusCollider.enabled = false;
    }

    public override void ToggleSelected()
    {
        base.ToggleSelected();
        areaRenderer.enabled = selected;
    }

    public override void TransitionToPlayPhase()
    {
        radiusCollider.enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isServer || isActive) return;
        if (other.tag == "Player")
        {
            isActive = true;
            RpcVisualizeTrap();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!isServer) return;
        if (other.tag == "Player")
        {
            other.GetComponent<CameraAvatar>().RootPlayer.GetComponent<Movement>().RpcSlow();
        }
    }

    [ClientRpc]
    private void RpcVisualizeTrap()
    {
        areaRenderer.enabled = true; //replace with playing anim
        GetComponent<MeshRenderer>().enabled = true;
    }
}