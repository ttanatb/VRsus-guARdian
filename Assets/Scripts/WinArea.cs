using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class WinArea : NetworkBehaviour
{
    public GameObject winUIPrefab;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        //only run checks on server
        if (!isServer)
            return;

        if (other.tag == "Player")
        {
            CmdSetWin();
            Destroy(this);
        }
    }

    [Command]
    void CmdSetWin()
    {
        GameObject ui = Instantiate(winUIPrefab, GameObject.Find("Canvas").transform);
        NetworkServer.Spawn(ui);
    }
}
