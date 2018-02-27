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
    private ParticleSystem alertParticles;
    public MeshRenderer radiusRenderer;
    private MeshRenderer meshRenderer;
    private static Transform[] players;
    private bool isActive = false;
    private SphereCollider radiusCollider;
    public override string TrapName { get { return "Proximity Sensor"; } }
    #endregion

    #region Init Logic
    // Use this for initialization
    void Start()
    {
        alertParticles = GetComponentInChildren<ParticleSystem>();
        meshRenderer = GetComponent<MeshRenderer>();
        if (isServer) meshRenderer.enabled = true;
    }

    public override void OnStartServer()
    {
        //gets the player to keep track for
        if (players == null)
        {
            PlayerInitializer[] playerScripts = FindObjectsOfType<PlayerInitializer>();
            players = new Transform[1];
            for (int i = 0; i < players.Length; i++)
            {
                if (playerScripts[i].PlayerType == PlayerType.VR)
                    players[0] = playerScripts[i].transform;
            }
        }

        //calc radius sqr
        transform.GetChild(0).localScale *= radius * 2;// transform.localScale.x;
        radiusCollider = gameObject.AddComponent<SphereCollider>();
        radiusCollider.radius = radius;
        radiusCollider.isTrigger = true;
        radiusCollider.enabled = false;
    }
    #endregion

    #region Life Cycle
    private void OnTriggerEnter(Collider other)
    {
        if (!isServer || isActive) return;
        if(other.tag == "Player")
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
        alertParticles.Play();
        if (!isServer)
        {
            meshRenderer.enabled = true;
        }
        meshRenderer.material.color = Color.red;
    }

    public override void TransitionToPlayPhase()
    {
        radiusCollider.enabled = true;
    }
    #endregion
}
