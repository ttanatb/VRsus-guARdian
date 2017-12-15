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

            Debug.Log("Collision with player! Relic count: " + combat.GetRelicCount());

            if (!combat.IsInvulnerable && combat.GetRelicCount() == 2 && manager.CurrGamePhase != GamePhase.Over)
            {
                Win(combat);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isServer)
            return;

        if (other.gameObject.tag == "Player")
        {
            Combat combat = other.gameObject.GetComponent<Combat>();
            if (!combat)
            {
                combat = other.gameObject.GetComponent<CameraAvatar>().rootPlayer.GetComponent<Combat>();
            }

            Debug.Log("Collision with player! Relic count: " + combat.GetRelicCount());

            if (!combat.IsInvulnerable && combat.GetRelicCount() == 2 && manager.CurrGamePhase != GamePhase.Over)
            {
                Win(combat);
            }
        }
    }

    private void Win(Combat combat)
    {
        manager.SetPhaseTo(GamePhase.Over);
        RpcAlertVRWin(combat.GetRelicCount());
    }

    [ClientRpc]
    void RpcAlertVRWin(int relicCount)
    {
        CanvasManager.Instance.SetPermanentMessage("VR player escaped with " + relicCount + " relic(s)");
    }
}