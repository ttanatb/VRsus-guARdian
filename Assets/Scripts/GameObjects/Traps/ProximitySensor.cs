using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// This script deals with the functionalities of the proximity sensor
/// 
/// Author: Tanat Boozayaangool
/// </summary>
public class ProximitySensor : TrapDefense
{
    #region Fields
    public float nearRange = 3;
    public float farRange = 5;

    private float nearRangeSqr;
    private float farRangeSqr;

    private static Transform[] players;

    public override string TrapName { get { return "Proximity Sensor"; } }
    #endregion

    #region Init Logic
    // Use this for initialization
    void Start()
    {
        //disable stuff for clients
        if (isServer) return;

        GetComponent<Renderer>().enabled = false;
        foreach (Collider c in GetComponents<Collider>())
        {
            if (!c.isTrigger)
            {
                c.enabled = false;
            }
        }
    }

    public override void OnStartServer()
    {
        //gets the player to keep track for
        if (players == null)
        {
            PlayerInitializer[] playerScripts = FindObjectsOfType<PlayerInitializer>();
            players = new Transform[1];
            for (int i = 0; i < players.Length; i++)
            {
                if (playerScripts[i].PlayerType == PlayerType.VR)
                    players[0] = playerScripts[i].transform;
            }
        }

        //swap the values if needed
        if (farRange < nearRange)
        {
            float temp = farRange;
            farRange = nearRange;
            nearRange = temp;
        }

        nearRangeSqr = Mathf.Pow(nearRange, 2);
        farRangeSqr = Mathf.Pow(farRange, 2);

        transform.GetChild(0).localScale *= nearRangeSqr;
        transform.GetChild(1).localScale *= farRangeSqr;
    }
    #endregion

    #region Life Cycle
    // Update is called once per frame
    void Update()
    {
        if (!isServer)
            return;

        //gets the closest distance to a player entity
        float closestDist = float.MaxValue;
        foreach (Transform t in players)
        {
            if (t.position.y < transform.position.y - transform.localScale.y / 2)
                continue;

            float dist = (t.position - transform.position).sqrMagnitude;
            if (dist < closestDist)
                closestDist = dist;
        }

        //determine the color (green to red)
        Color c = Color.black;
        float halfRange = (farRangeSqr - nearRangeSqr) / 2f;
        c.r = Mathf.Lerp(1, 0, (closestDist - nearRangeSqr) / halfRange);
        c.g = Mathf.Lerp(0, 1, (closestDist - nearRangeSqr + halfRange) / (halfRange * 2f));
        GetComponent<Renderer>().material.color = c;
    }

    /// <summary>
    /// Toggles the renderers of the radius
    /// </summary>
    public override void ToggleSelected()
    {
        base.ToggleSelected();

        for (int i = 0; i < transform.childCount; i++)
            transform.GetChild(i).GetComponent<Renderer>().enabled = selected;
    }
    #endregion
}
