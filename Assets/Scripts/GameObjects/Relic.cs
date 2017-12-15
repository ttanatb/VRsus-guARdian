using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class Relic : NetworkBehaviour
{
    private GameObject[] walls;
    private Vector3[] lerpPos;
    private GameObject associatedTower;

    public Renderer relicRenderer;

    [SyncVar]
    private bool isErecting = false;

    public void Init(GameObject[] walls, GameObject tower)
    {
        this.walls = walls;
        lerpPos = new Vector3[walls.Length];
        associatedTower = tower;

        for (int i = 0; i < lerpPos.Length; i++)
        {
            walls[i].GetComponent<Renderer>().enabled = false;
            lerpPos[i] = new Vector3(walls[i].transform.position.x,
                associatedTower.transform.position.y + associatedTower.transform.localScale.y / 2 + Random.Range(walls[i].transform.localScale.y / 16, walls[i].transform.localScale.y / 2),
                walls[i].transform.position.z);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isServer || isErecting)
            return;

        if (collision.gameObject.tag == "Player")
        {
            Combat combat = collision.gameObject.GetComponent<Combat>();
            if (!combat)
            {
                combat = collision.gameObject.GetComponent<CameraAvatar>().rootPlayer.GetComponent<Combat>();
            }

            if (!combat.IsInvulnerable)
            {
                combat.GainRelic();
                isErecting = true;
                CmdRelicStolen();
                RpcStealRelic();
                relicRenderer.enabled = false;
                for (int i = 0; i < lerpPos.Length; i++)
                {
                    //walls[i].GetComponent<Renderer>().enabled = true;
                }
            }
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!isServer || isErecting)
            return;
        if (other.gameObject.tag == "Player")
        {
            Combat combat = other.gameObject.GetComponent<Combat>();
            if (!combat)
            {
                combat = other.gameObject.GetComponent<CameraAvatar>().rootPlayer.GetComponent<Combat>();
            }

            if (!combat.IsInvulnerable)
            {
                combat.GainRelic();
                isErecting = true;
                CmdRelicStolen();
                RpcStealRelic();
                relicRenderer.enabled = false;
                for (int i = 0; i < lerpPos.Length; i++)
                {
                    //walls[i].GetComponent<Renderer>().enabled = true;
                }
            }
        }
    }

    private void Update()
    {
        if (isErecting)
        {
            GetComponent<Renderer>().enabled = false;
        }
    }

    [Command]
    private void CmdRelicStolen()
    {
        CanvasManager.Instance.SetMessage("A relic was stolen!");
    }

    [ClientRpc]
    private void RpcStealRelic()
    {
        relicRenderer.enabled = false;
        CanvasManager.Instance.SetMessage("Stole a relic!");
    }
}
