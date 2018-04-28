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
        //healthBarPivot = transform.GetChild(0).GetChild(0);
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

    }

    public void ResetHealth()
    {
        foreach(GameObject obj in lives)
        {
            obj.SetActive(true);
        }
    }

    public void DecrementHealth()
    {
        int index = player.health - 1;
        if (index < 0) return;
        lives[index].SetActive(false);
    }

    #endregion
}
