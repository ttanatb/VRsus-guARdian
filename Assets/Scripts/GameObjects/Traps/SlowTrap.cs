using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SlowTrap : TrapDefense
{
    [SyncVar]
    bool isActive = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!isServer) return;

        if (other.tag == "Player")
        {
            if (!isActive)
                CmdTriggerTrap();

            if (isActive)
            {
                other.GetComponent<Movement>().IsSlowed = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!isServer) return;

        if (other.tag == "Player" && isActive)
        {
            other.GetComponent<Movement>().IsSlowed = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player" && !isActive) 
            CmdTriggerTrap();
    }

    [Command]
    private void CmdTriggerTrap()
    {
        isActive = true;
        GetComponentInChildren<Renderer>().enabled = true;
    }
}
