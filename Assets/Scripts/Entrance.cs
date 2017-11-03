using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Entrance : NetworkBehaviour
{
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

            if (combat.GetRelicCount() > 0)
            {
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