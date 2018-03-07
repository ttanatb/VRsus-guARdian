using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// This script deals with the functionalities of the proximity sensor
/// 
/// Author: Tanat Boozayaangool
/// </summary>
public class ProximitySensor : TrapDefense
{
    #region Fields
    public float radius = 3;
    public ParticleSystem[] alertParticles;
    public MeshRenderer radiusRenderer;
    public MeshRenderer trapMeshRenderer;
    private bool isActive = false;
    private SphereCollider radiusCollider;

    private BoxCollider selectionCollider;
    public override string TrapName { get { return "Proximity Sensor"; } }
    #endregion

    #region Init Logic
    // Use this for initialization
    void Start()
    {
        selectionCollider = GetComponent<BoxCollider>();
        if (!isServer)
        {
            selectionCollider.enabled = false;
            trapMeshRenderer.enabled = false;
            radiusRenderer.enabled = false;
        }
    }

    public override void OnStartServer()
    {
        //calc radius sqr
        radiusRenderer.transform.localScale *= radius * 2 / 10f;// transform.localScale.x;
        radiusCollider = gameObject.AddComponent<SphereCollider>();
        radiusCollider.radius = radius / 10f;
        radiusCollider.isTrigger = true;
        radiusCollider.enabled = false;
    }
    #endregion

    #region Life Cycle
    private void OnTriggerEnter(Collider other)
    {
        if (!isServer || isActive) return;
        if (other.tag == "Player")
        {
            isActive = true;
            RpcTriggerAlarm();
        }
    }

    /// <summary>
    /// Toggles the renderers of the radius
    /// </summary>
    public override void ToggleSelected()
    {
        base.ToggleSelected();
        radiusRenderer.enabled = selected;
    }

    [ClientRpc]
    private void RpcTriggerAlarm()
    {
        foreach (ParticleSystem p in alertParticles)
            p.Play();
        if (!isServer)
        {
            trapMeshRenderer.enabled = true;
        }
        trapMeshRenderer.material.color = Color.red;
    }

    public override void TransitionToPlayPhase()
    {
        radiusCollider.enabled = true;
        selectionCollider.enabled = false;
    }
    #endregion
}
