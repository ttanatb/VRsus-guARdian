using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TrapDefense : NetworkBehaviour
{

    [ClientRpc]
    public virtual void RpcDisable()
    {

    }

    public virtual void ToggleSelected()
    {

    }

    public virtual string TrapName
    {
        get { return "Trap"; }
    }
}
