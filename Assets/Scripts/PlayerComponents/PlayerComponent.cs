using UnityEngine.Networking;

/// <summary>
/// Base class for all functional scripts that are attached to the player
/// 
/// Author: Tanat Boozayaangool
/// </summary>
public abstract class PlayerComponent : NetworkBehaviour
{
    #region Fields
    //Fields
    protected PlayerType playerType;
    protected PlayerInitializer player;
    #endregion

    #region Init & Destruction
    /// <summary>
    /// Links/creates scripts that needed to be destroyed on death
    /// </summary>
    protected abstract void InitObj();

    /// <summary>
    /// Initializes references to the respective player script
    /// </summary>
    /// <param name="playerType">The type of player this component belongs to</param>
    /// <param name="player">The respective PlayerInitializer script</param>
    public void InitMemberFields(PlayerType playerType, PlayerInitializer player)
    {
        this.playerType = playerType;
        this.player = player;
    }

    /// <summary>
    /// Makes sure to link appropriate objects to destroy before actually destroying it
    /// </summary>
    public virtual void Destroy()
    {
        InitObj();
        Destroy(this);
    }
    #endregion
}