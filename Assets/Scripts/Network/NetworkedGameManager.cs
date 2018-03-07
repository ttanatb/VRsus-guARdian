using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class NetworkedGameManager : NetworkManager
{
    public bool isArtTest = false;
    public bool isHookshotTest = false;
    public GameObject[] playerPrefabs;

    public GameObject practiceAreaPrefab;
    private GameObject practiceAreaObj;
    private PraticeArea practiceArea;

    public bool isVR = false;

    private int index = -1;
    // 0 - Default Player
    // 1 - AR Player
    // 2 - VR Player
    // 3 - PC Player

    private void Start()
    {
        if (isArtTest || isHookshotTest)
        {
            StartHost();
        }
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        //Debug.Log("OnClientConnect is called -- now sending data about:");
        //switch (index)
        //{
        //    case 0: Debug.Log("Default Player"); break;
        //    case 1: Debug.Log("AR Player"); break;
        //    case 2: Debug.Log("VR Player"); break;
        //    case 3: Debug.Log("PC Player"); break;
        //}
        if (index != 1)
        {
            if (isVR) index = 2;
            else index = 3;
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
            //Debug.Log("playerIndex is now " + playerIndex);
        }
        //switch (playerIndex)
        //{
        //    case 0: Debug.Log("Default Player"); break;
        //    case 1: Debug.Log("AR Player"); break;
        //    case 2: Debug.Log("VR Player"); break;
        //    case 3: Debug.Log("PC Player"); break;
        //}

        Vector3 position = Vector3.zero;
        if (playerIndex != 1)
        {
            position = practiceArea.spawnPos.position;
        }
        else
            NetworkServer.Spawn(practiceAreaObj);

        GameObject playerObj = Instantiate(playerPrefabs[playerIndex], position, Quaternion.identity);
        NetworkServer.AddPlayerForConnection(conn, playerObj, playerControllerId);
    }

    public override void OnStartHost()
    {
        //Debug.Log("OnStartHost is Called!");
        if (isHookshotTest)
            index = 3;
        else index = 1;
        base.OnStartHost();

    }

    public override void OnStartServer()
    {
        Debug.Log("On start server");
        base.OnStartServer();
        practiceAreaObj = Instantiate(practiceAreaPrefab);
        practiceArea = practiceAreaObj.GetComponent<PraticeArea>();
        //NetworkServer.Spawn(practiceAreaObj);
    }

    public override void OnStartClient(NetworkClient client)
    {
        //Debug.Log("OnStartClient is Called!");
        if (index == -1)
        {
            if (isVR) index = 2;
            else index = 3;
        }
        base.OnStartClient(client);
    }
}
