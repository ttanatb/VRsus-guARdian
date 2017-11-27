using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Entrance : NetworkBehaviour
{
    private GameManager manager;

    public override void OnStartServer()
    {
        manager = FindObjectOfType<GameManager>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isServer)
            return;

        if (collision.gameObject.tag == "Player")
        {
            Combat combat = collision.gameObject.GetComponent<Combat>();
            if (!combat)
            {
                combat = collision.gameObject.GetComponent<CameraAvatar>().rootPlayer.GetComponent<Combat>();
            }

            if (combat.GetRelicCount() > 0 && manager.CurrGamePhase != GamePhase.Over)
            {
                manager.SetPhaseTo(GamePhase.Over);
                RpcAlertVRWin(combat.GetRelicCount());
            }
        }
    }

    [ClientRpc]
    void RpcAlertVRWin(int relicCount)
    {
        CanvasManager.Instance.SetPermanentMessage("VR player escaped with " + relicCount + " relic(s)");
    }
}