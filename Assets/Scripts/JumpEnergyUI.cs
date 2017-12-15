using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpEnergyUI : MonoBehaviour
{
    Transform healthBarPivot;
    Movement player;
    float maxEnergy;

    // Use this for initialization
    void Start()
    {
        healthBarPivot = transform.GetChild(0).GetChild(0);
    }

    public void Init(Movement combat)
    {
        player = combat;
        maxEnergy = player.jumpEnergyMax;
    }

    // Update is called once per frame
    void Update()
    {
        float healthPercentage = player.CurrJumpEnergy / maxEnergy;
        if (healthPercentage < 0)
            healthPercentage = 0f;

        Vector3 scale = healthBarPivot.localScale;
        scale.x = Mathf.Lerp(scale.x, healthPercentage, Time.deltaTime * 20f);

        healthBarPivot.localScale = scale;
    }
}
