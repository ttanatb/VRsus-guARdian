using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkedGameManager : NetworkManager
{
    public bool isArtTest = false;
    public GameObject gameManagerPrefab;

    private void Start()
    {
        if (isArtTest)
        {
            StartHost();
            //StartServer();
            //networkAddress = "127.0.0.1";
            //StartClient();
        }
    }

    public override void OnServerConnect(NetworkConnection conn)
    {
        //Debug.Log("Player number " + (numPlayers + 1) + " has connected to the server");
        base.OnServerConnect(conn);
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        //Debug.Log("Someone disconnected. " + numPlayers + " players are left ");
        base.OnServerDisconnect(conn);
    }
}
