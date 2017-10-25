using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GamePhase
{
    Scanning,
    Placing,
    Playing,
}

public class GameManager : MonoBehaviour
{

    private GamePhase currGamePhase = GamePhase.Scanning;

    public GamePhase CurrGamePhase { get { return currGamePhase; } }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
