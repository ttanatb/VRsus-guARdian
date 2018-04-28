using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RelicCounterUI : MonoBehaviour
{
    private Text text;

    // Use this for initialization
    void Awake()
    {
        text = GetComponentInChildren<Text>();
    }

    public void SetCount(int count)
    {
        text.text = count + "/2";
    }
}
