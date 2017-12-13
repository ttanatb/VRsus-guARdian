using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TrapDefense : NetworkBehaviour
{
    protected bool selected = false;

    [ClientRpc]
    public virtual void RpcDisable()
    {

    }

    public virtual void ToggleSelected()
    {
        selected = !selected;
    }

    public virtual string TrapName
    {
        get { return "Trap"; }
    }
}
