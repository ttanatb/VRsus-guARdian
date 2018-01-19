using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// A wall that becomes visible once the player collides with it.
/// 
/// Author: Tanat Boozayaangool
/// </summary>
public class InvisibleWall : TrapDefense
{
    //fields
    bool isActive = false;

    public override string TrapName { get { return "Invisible Wall"; } }

    // Use this for initialization
    void Start()
    {
        if (!isLocalPlayer)
            GetComponent<Renderer>().enabled = false;
    }

    // Collision detection
    private void OnTriggerEnter(Collider other)
    {
        if (!isServer || isActive) return;

        if (other.gameObject.tag == "Player")
        {
            GetComponent<Renderer>().enabled = true;
            RpcEnable();
            GetComponent<Rigidbody>().isKinematic = true;
        }
    }

    /// <summary>
    /// Enables the trap for the clients (VR Player)
    /// </summary>
    [ClientRpc]
    private void RpcEnable()
    {
        GetComponent<Renderer>().enabled = true;
        GetComponent<Rigidbody>().isKinematic = true;
    }
}
