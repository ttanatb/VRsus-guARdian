using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class NetworkedGameManager : NetworkManager
{
    public bool isArtTest = false;
    public GameObject[] playerPrefabs;

    private int index = -1;
    // 0 - Default Player
    // 1 - AR Player
    // 2 - VR Player
    // 3 - PC Player

    private void Start()
    {
        if (isArtTest)
        {
            StartHost();
        }
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        Debug.Log("OnClientConnect is called -- now sending data about:");
        switch (index)
        {
            case 0: Debug.Log("Default Player"); break;
            case 1: Debug.Log("AR Player"); break;
            case 2: Debug.Log("VR Player"); break;
            case 3: Debug.Log("PC Player"); break;
        }


        IntegerMessage msg = new IntegerMessage(index);
        ClientScene.AddPlayer(conn, 0, msg);
    }


    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId, NetworkReader extraMessageReader)
    {
        Debug.Log("OnServerAddPlayer is called -- now adding...");

        int playerIndex = 0;
        if (extraMessageReader.Length > 0)
        {
            playerIndex = extraMessageReader.ReadMessage<IntegerMessage>().value;
            Debug.Log("playerIndex is now " + playerIndex);
        }
        switch (playerIndex)
        {
            case 0: Debug.Log("Default Player"); break;
            case 1: Debug.Log("AR Player"); break;
            case 2: Debug.Log("VR Player"); break;
            case 3: Debug.Log("PC Player"); break;
        }

        GameObject playerObj = Instantiate(playerPrefabs[playerIndex], Vector3.zero, Quaternion.identity);
        NetworkServer.AddPlayerForConnection(conn, playerObj, playerControllerId);
    }

    public override void OnStartHost()
    {
        Debug.Log("OnStartHost is Called!");
        index = 1;
        base.OnStartHost();
    }

    public override void OnStartClient(NetworkClient client)
    {
        Debug.Log("OnStartClient is Called!");
        if (index == -1)
        {
            if (UnityEngine.XR.XRSettings.enabled) index = 2;
            else index = 3;
        }
        base.OnStartClient(client);
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
