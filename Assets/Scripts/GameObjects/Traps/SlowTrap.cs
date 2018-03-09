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
    private BoxCollider selectorCollider;

    public MeshRenderer swampMeshRenderer;

    public Animator trapEnableAnim;
    public ParticleSystem pSystem;

    public override string TrapName { get { return "Slow Trap"; } }

    private void Start()
    {
        selectorCollider = GetComponent<BoxCollider>();
        transform.GetChild(0).localScale *= radius * 2f / 10f;

        if (!isServer)
        {
            selectorCollider.enabled = false;
            swampMeshRenderer.enabled = false;
            areaRenderer.enabled = false;
        }
    }

    public override void OnStartServer()
    {
        swampMeshRenderer.enabled = true;
        radiusCollider = gameObject.AddComponent<SphereCollider>();
        radiusCollider.radius = radius / 10f;
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
        selectorCollider.enabled = false;
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
            Movement movement = other.transform.parent.GetComponent<Movement>();
            if (movement.isOnFloor)
                movement.RpcSlow();
        }
    }

    [ClientRpc]
    private void RpcVisualizeTrap()
    {
        swampMeshRenderer.enabled = true;
        //areaRenderer.enabled = true; //replace with playing anim
        trapEnableAnim.SetBool("isActivated", true);
        pSystem.Play();
    }
}