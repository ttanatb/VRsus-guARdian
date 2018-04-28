using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LaserUI : MonoBehaviour
{
    Image cooldownImage;
    ARCombat arCombat;

    // Use this for initialization
    void Start()
    {
        cooldownImage = GetComponent<Image>();
    }
    
    public void Init(ARCombat combat)
    {
        arCombat = combat;
    }

    // Update is called once per frame
    void Update()
    {
        cooldownImage.fillAmount = arCombat.LaserTimer;
    }
}
