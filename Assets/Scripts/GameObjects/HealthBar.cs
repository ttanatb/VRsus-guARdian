using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is the visual health display for the VR player
/// 
/// Author: Tanat Boozayaangool
/// </summary>
public class HealthBar : MonoBehaviour
{
    #region Fields
    private VRCombat player;    //player with the health

    //health stuff
    private int maxHealth;
    private float healthPercentage;
    private Transform healthBarPivot;

    private PlayerType typeOfPlayer;
    private Transform playerAvatar;

    public float displayDistAR = 1f;
    public float displayDistVR = 1f;

    //renderer
    public Renderer bgRenderer;
    private Renderer render;
    #endregion

    #region Init Logic
    // Use this for initialization
    void Start()
    {
        healthBarPivot = transform.GetChild(0);
        render = GetComponent<Renderer>();
    }

    /// <summary>
    /// Works as the psuedo-constructor
    /// </summary>
    /// <param name="combat">Combat player with the health</param>
    /// <param name="playerType">Type of player</param>
    /// <param name="avatar">The avatar of the player</param>
    public void Init(VRCombat combat, PlayerType playerType, Transform avatar)
    {
        player = combat;
        typeOfPlayer = playerType;
        playerAvatar = avatar;
        maxHealth = player.health;
    }
    #endregion

    #region Life Cycle
    // Update is called once per frame
    void Update()
    {
        //doesn't display if player is invulnerable
        if (player.IsInvulnerable)
        {
            render.enabled = false;
            bgRenderer.enabled = false;
            return;
        }
        else
        {
            render.enabled = true;
            bgRenderer.enabled = true;
        }
        
        //places the health bar near the avatar
        if (playerAvatar)
        {
            if (typeOfPlayer == PlayerType.AR)
                transform.position = playerAvatar.position + Vector3.down * displayDistAR;
            else
                transform.position = playerAvatar.position + Vector3.up * displayDistVR;
        }
        else Destroy(gameObject);

        //scales according to health
        if (player)
        {
            healthPercentage = player.health / (float)maxHealth;
            if (healthPercentage < 0)
                healthPercentage = 0f;

            Vector3 scale = healthBarPivot.localScale;
            scale.x = Mathf.Lerp(scale.x, healthPercentage, Time.deltaTime * 20f);

            healthBarPivot.localScale = scale;
        }

        //faces the camera
        if (Camera.main)
            transform.forward = -Camera.main.transform.forward;
    }
    #endregion
}
