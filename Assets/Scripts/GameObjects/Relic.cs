using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Handles player collecting relic and related stuff
/// 
/// Author: Tanat Boozayaangool
/// </summary>
public class Relic : NetworkBehaviour
{
    public Renderer relicRenderer;
    private bool hasBeenStolen = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!isServer || hasBeenStolen)
            return;

        if (other.gameObject.tag == "Player")
        {
            VRCombat combat = other.gameObject.GetComponent<VRCombat>();
            if (!combat)
                combat = other.gameObject.GetComponent<CameraAvatar>().RootPlayer.GetComponent<VRCombat>();

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

        if (isServer)
            CanvasManager.Instance.SetMessage("A relic was stolen!");
        else CanvasManager.Instance.SetMessage("Stole a relic!");
    }
}