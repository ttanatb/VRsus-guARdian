using UnityEngine.Networking;

/// <summary>
/// Base class for the combat class
/// 
/// Author: Tanat Boozayaangool
/// </summary>
public abstract class Combat : PlayerComponent
{
    protected override void InitObj() { }

    /// <summary>
    /// Function for when a player takes damage
    /// </summary>
    [Server]
    public virtual void TakeDamage() { }
}
