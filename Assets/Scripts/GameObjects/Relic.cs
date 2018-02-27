using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

/// <summary>
/// Handles player collecting relic and related stuff
/// 
/// Author: Tanat Boozayaangool
/// </summary>
public class Relic : NetworkBehaviour
{
    public Renderer relicRenderer;
    private bool hasBeenStolen = false;
    private List<Entrance> entrances;

    public override void OnStartServer()
    {
        entrances = new List<Entrance>();
    }

    public void AddEntrance(Entrance entrance)
    {
        entrances.Add(entrance);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!isServer || hasBeenStolen)
            return;

        if (other.gameObject.tag == "Player")
        {
            VRCombat combat = other.transform.parent.GetComponent<VRCombat>();
            if (!combat.IsInvulnerable)
            {
                hasBeenStolen = true;
                combat.GainRelic();
                RpcStealRelic();
            }
        }
    }

    [ClientRpc]
    private void RpcStealRelic()
    {
        if (relicRenderer)
            relicRenderer.enabled = false;


        if (entrances != null)
        {
            foreach (Entrance e in entrances)
            {
                e.RpcActivate();
            }
        }


        if (isServer)
            CanvasManager.Instance.SetMessage("A relic was stolen!");
        else CanvasManager.Instance.SetMessage("Stole a relic!");
    }
}