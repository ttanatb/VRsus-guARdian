using UnityEngine;

/// <summary>
/// Handles the UI displaying the current health of the player
/// 
/// Author: Tanat Boozayaangool
/// </summary>
public class HealthBarUI : MonoBehaviour
{
    #region Fields
    private Transform healthBarPivot;
    private VRCombat player;
    private int maxHealth;

	[SerializeField]
	private GameObject[] lives;
    #endregion

    #region Init Logic
    // Use this for initialization
    void Start()
    {
        healthBarPivot = transform.GetChild(0).GetChild(0);
    }

    /// <summary>
    /// Used as a 'constructor' of sorts
    /// </summary>
    /// <param name="combat">The player with the health</param>
    public void Init(VRCombat combat)
    {
		for (int i = 0; i < 3; i++) {
			lives [i].SetActive (true);
		}
        player = combat;
        maxHealth = player.health;
    }
    #endregion

    #region Life Cycle
    // Update is called once per frame
    void Update()
    {
        //calculates percentage
        //float healthPercentage = player.health / (float)maxHealth;
        //if (healthPercentage < 0f)
        //    healthPercentage = 0f;
        //else if (healthPercentage > 1f)
        //    healthPercentage = 1f;


        //sets scale
        //Vector3 scale = healthBarPivot.localScale;
        //scale.x = Mathf.Lerp(scale.x, healthPercentage, Time.deltaTime * 20f);
        //healthBarPivot.localScale = scale;
		switch (player.health) {
		case 2:
			lives [2].SetActive (false);
			break;
		case 1:
			lives [1].SetActive (false);
			break;
		case 0:
			lives [0].SetActive (false);
			break;
		}

    }
    #endregion
}
