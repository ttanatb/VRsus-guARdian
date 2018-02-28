using System;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Manages the entrances and what happens when you hit it
/// 
/// Author: Tanat Boozayaangool
/// </summary>
public class Entrance : NetworkBehaviour
{
    //Field(s)
    private ARSetUp manager;
    private Renderer mRenderer;
    private BoxCollider bCollider;

    private void Awake()
    {
        mRenderer = GetComponent<MeshRenderer>();
        bCollider = GetComponent<BoxCollider>();
    }

    //Init
    public override void OnStartServer()
    {
        manager = FindObjectOfType<ARSetUp>();
        if (!mRenderer)
        {
            mRenderer = GetComponent<MeshRenderer>();
        }
        mRenderer.enabled = false;
    }

    /// <summary>
    /// Handles when a VR player collides with the entrance
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (!isServer)
            return;

        //
        if (other.gameObject.tag == "Player")
        {
            VRCombat combat = other.transform.parent.GetComponent<VRCombat>();
            if (!combat.IsInvulnerable && combat.GetRelicCount() == 2 && manager.CurrGamePhase != GamePhase.Over)
                Win(combat);
        }
    }

    /// <summary>
    /// Appropriately adjust values to reflect winning
    /// </summary>
    /// <param name="combat">The VRcombat with the relics</param>
    private void Win(VRCombat combat)
    {
        manager.SetPhaseTo(GamePhase.Over);
        RpcAlertVRWin(combat.GetRelicCount());
    }

    [ClientRpc]
    public void RpcActivate()
    {
        if (mRenderer)
        {
            mRenderer.enabled = true;
            bCollider.enabled = true;
        }
    }
    public void Deactivate()
    {
        if (mRenderer)
        {
            mRenderer.enabled = false;
            bCollider.enabled = false;
        }
    }

    /// <summary>
    /// Sends a message to announce winning
    /// </summary>
    /// <param name="relicCount">The count of relics</param>
    [ClientRpc]
    void RpcAlertVRWin(int relicCount)
    {
        CanvasManager.Instance.SetPermanentMessage("VR player escaped with " + relicCount + " relic(s)");
    }
}