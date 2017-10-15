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
        Debug.Log(other.gameObject + other.tag);

        if (other.tag == "Player")
        {
            CmdSetWin();
            RpcSetWin();

            //GameObject ui = Instantiate(winUIPrefab, GameObject.Find("Canvas").transform);
            GameObject ui = Instantiate(winUIPrefab, GameObject.Find("Canvas").transform);
            NetworkServer.Spawn(ui);
        }
    }

    [Command]
    void CmdSetWin()
    {
        GameObject ui = Instantiate(winUIPrefab, GameObject.Find("Canvas").transform);
#if UNITY_IOS
        ui.GetComponent<Text>().text = "You lose!";
#endif
        NetworkServer.Spawn(ui);
    }

    [ClientRpc]
    void RpcSetWin()
    {
        GameObject ui = Instantiate(winUIPrefab, GameObject.Find("Canvas").transform);
#if UNITY_IOS
        ui.GetComponent<Text>().text = "You lose!";
#endif
        NetworkServer.Spawn(ui);
    }
}
