using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// The base class for all defense traps
/// 
/// Author: Tanat Boozayaangool
/// </summary>
public abstract class TrapDefense : NetworkBehaviour
{
    protected bool selected = false; //wether the trap is currently being selected

    /// <summary>
    /// The name of the trap (label)
    /// </summary>
    public virtual string TrapName { get { return "Trap"; } }

    /// <summary>
    /// For disabling the trap
    /// </summary>
    [ClientRpc]
    public virtual void RpcDisable() { }

    /// <summary>
    /// Toggle the selection of the trap
    /// </summary>
    public virtual void ToggleSelected()
    {
        selected = !selected;
    }
}
