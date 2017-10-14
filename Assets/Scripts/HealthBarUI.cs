using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBarUI : MonoBehaviour
{
    Transform healthBarPivot;
    Combat player;
    int maxHealth;

    // Use this for initialization
    void Start()
    {
        healthBarPivot = transform.GetChild(0).GetChild(0);
    }

    public void Init(Combat combat)
    {
        player = combat;
        maxHealth = player.health;
    }

    // Update is called once per frame
    void Update()
    {
        float healthPercentage = (float)player.health / (float)maxHealth;
        if (healthPercentage < 0)
            healthPercentage = 0f;

        Vector3 scale = healthBarPivot.localScale;
        scale.x = Mathf.Lerp(scale.x, healthPercentage, Time.deltaTime * 20f);

        healthBarPivot.localScale = scale;
    }
}
