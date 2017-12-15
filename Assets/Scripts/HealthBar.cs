using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    private Combat player;
    int maxHealth;
    float healthPercentage;

    private Transform healthBarPivot;

    private PlayerType typeOfPlayer;
    private Transform playerAvatar;

    public float displayDistAR = 1f;
    public float displayDistVR = 1f;

    public Renderer bgRenderer;
    private Renderer rendererr;

    // Use this for initialization
    void Start()
    {
        healthBarPivot = transform.GetChild(0);
        rendererr = GetComponent<Renderer>();
    }

    public void Init(Combat combat, PlayerType playerType, Transform avatar)
    {
        player = combat;
        typeOfPlayer = playerType;
        playerAvatar = avatar;
        maxHealth = player.health;
    }

    // Update is called once per frame
    void Update()
    {
        if (player.IsInvulnerable)
        {
            rendererr.enabled = false;
            bgRenderer.enabled = false;
            return;
        }
        else
        {
            rendererr.enabled = true;
            bgRenderer.enabled = true;
        }

        if (playerAvatar)
        {
            if (typeOfPlayer == PlayerType.AR)
            {
                transform.position = playerAvatar.position + Vector3.down * displayDistAR;
            }
            else
            {
                transform.position = playerAvatar.position + Vector3.up * displayDistVR;
            }
        }
        else
        {
            //Debug.LogError("Missing Avatar for " + gameObject.name);
            Destroy(gameObject);
        }

        if (player)
        {
            healthPercentage = (float)player.health / (float)maxHealth;
            if (healthPercentage < 0)
                healthPercentage = 0f;

            Vector3 scale = healthBarPivot.localScale;
            scale.x = Mathf.Lerp(scale.x, healthPercentage, Time.deltaTime * 20f);

            healthBarPivot.localScale = scale;
        }
        else
        {
            //Debug.LogError("Missing Player combat for " + gameObject.name);
            Destroy(gameObject);
        }

        if (Camera.main)
        {
            transform.forward = -Camera.main.transform.forward;
        }
        else
        {
            //Debug.LogError("Missing Main Camera for " + gameObject.name);
            Destroy(gameObject);
        }
    }
}
