using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkedGameManager : NetworkManager
{
    public GameObject gameManagerPrefab;

    public override void OnServerConnect(NetworkConnection conn)
    {
        Debug.Log("Player number " + (numPlayers + 1) + " has connected to the server");
        base.OnServerConnect(conn);
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        Debug.Log("Someone disconnected. " + numPlayers + " players are left ");
        base.OnServerDisconnect(conn);
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        GameObject go = Instantiate(gameManagerPrefab);
        NetworkServer.Spawn(go);
        base.OnClientConnect(conn);
    }
}
