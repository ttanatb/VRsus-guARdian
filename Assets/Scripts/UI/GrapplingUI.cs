using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GrapplingUI : MonoBehaviour
{
    public Grappling grappling;
    private Image image;
    public Color shootColor;
    public Color attachedColor;
    public Color retractColor;
    private Color defaultColor;

    private void Start()
    {
        image = GetComponent<Image>();
        defaultColor = Color.white;
    }

    // Update is called once per frame
    void Update()
    {
        int state = grappling.State;
        if (state == 0)
        {
            image.color = defaultColor;
        }
        else if (state == 1)
        {
            image.color = shootColor;
        }
        else if (state == 4)
        {
            image.color = retractColor;
        }
        else
        {
            image.color = attachedColor;
        }

        //private int state = 0;
        // 0 - inactive
        // 1 - shooting out
        // 2 - plyrTraveling
        // 3 - attached
        // 4 - retracting
    }
}
