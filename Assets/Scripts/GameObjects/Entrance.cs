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

    //Init
    public override void OnStartServer()
    {
        manager = FindObjectOfType<ARSetUp>();
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
            VRCombat combat = other.gameObject.GetComponent<VRCombat>();
            if (!combat)
                combat = other.gameObject.GetComponent<CameraAvatar>().rootPlayer.GetComponent<VRCombat>();

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