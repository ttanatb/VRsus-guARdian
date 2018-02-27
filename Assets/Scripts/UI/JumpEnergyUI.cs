using UnityEngine;

/// <summary>
/// Manages the UI for the jump energy
/// 
/// Author: Tanat Boozayaangool
/// </summary>
public class JumpEnergyUI : MonoBehaviour
{
    #region Fields
    private Transform healthBarPivot;
    private Movement player;
    #endregion

    #region Init Logic
    // Use this for initialization
    void Start()
    {
        healthBarPivot = transform.GetChild(0).GetChild(0);
    }

    /// <summary>
    /// A function that acts as the 'constructor' of sorts
    /// </summary>
    /// <param name="combat"></param>
    public void Init(Movement combat)
    {
        player = combat;
    }
    #endregion

    #region Life Cycle
    // Update is called once per frame
    void Update()
    {
        //calculate percentage
        //float energyPercentage = player.CurrJumpEnergy / player.jumpEnergyMax;
        //if (energyPercentage < 0f)
        //    energyPercentage = 0f;
        //else if (energyPercentage > 1f)
        //    energyPercentage = 1f;

        ////set the scale
        //Vector3 scale = healthBarPivot.localScale;
        //scale.x = Mathf.Lerp(scale.x, energyPercentage, Time.deltaTime * 20f);
        //healthBarPivot.localScale = scale;
    }
    #endregion
}
