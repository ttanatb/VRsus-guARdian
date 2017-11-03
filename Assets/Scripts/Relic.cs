using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class Relic : NetworkBehaviour
{
    private GameObject[] walls;
    private Vector3[] lerpPos;
    private GameObject associatedTower;
    private bool isErecting = false;

    public void Init(GameObject[] walls, GameObject tower)
    {
        this.walls = walls;
        lerpPos = new Vector3[walls.Length];
        associatedTower = tower;

        for (int i = 0; i < lerpPos.Length; i++)
        {
            lerpPos[i] = new Vector3(walls[i].transform.position.x,
                associatedTower.transform.position.y + associatedTower.transform.localScale.y / 2 + Random.Range(walls[i].transform.localScale.y / 16, walls[i].transform.localScale.y / 2),
                walls[i].transform.position.z);
        }
    }

    private void OnCollisionStay(Collision collision)
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
                AlertRelicStolen();
            }
        }
    }

    private void Update()
    {
        if (!isServer)
            return;

        if (isErecting)
        {
            for (int i = 0; i < walls.Length; i++)
            {
                walls[i].transform.position = Vector3.Lerp(walls[i].transform.position, lerpPos[i], Time.deltaTime);
            }

            transform.position = Vector3.Lerp(transform.position, transform.position - Vector3.up * 2f, Time.deltaTime);
        }
    }

    [Server]
    private void AlertRelicStolen()
    {
        CanvasManager.Instance.SetMessage("A relic was stolen!");
    }
}
