using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public enum GamePhase
{
    Scanning = 0,
    Placing = 1,
    Playing = 2,
}

public class GameManager : NetworkBehaviour
{
    [SyncVar]
    private int currGamePhase = 0;

    public GamePhase CurrGamePhase { get { return (GamePhase)currGamePhase; } }

    public GameObject PlaneGeneratorPrefab;

    // Use this for initialization
    void Start()
    {

    }

    public override void OnStartServer()
    {
#if UNITY_IOS
        Instantiate(PlaneGeneratorPrefab);
        CanvasManager.Instance.SetUI(this);
#endif
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetPhaseTo(GamePhase newPhase)
    {
        currGamePhase = (int)newPhase;
        CanvasManager.Instance.SetUI(this);
    }
}
