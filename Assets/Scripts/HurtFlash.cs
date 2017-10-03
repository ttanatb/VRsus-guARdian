using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HurtFlash : MonoBehaviour
{

    //the speed in which the red increases in opacity
    [Range(0f, 1f)]
    public float flashSpeed;

    //the speed in which the red fades away
    [Range(0f, 1f)]
    public float fadeSpeed;

    //the amount of time in which the red stays on screen
    public float waitTime = 3f;

    private Image image;
    private float alpha = 0;
    private IEnumerator fadeCouroutine;

    //start
    void Start()
    {
        image = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        //get current color
        Color c = image.color;

        //clamp the alpha
        if (alpha > 1f)
        {
            alpha = 1f;
        }
        else if (alpha < 0f)
        {
            alpha = 0f;
        }

        //set the alpha
        c.a = alpha;
        image.color = c;
    }

    //public function that's called to flash the screen
    public void FlashRed()
    {
        StopCoroutine(fadeCouroutine);
        fadeCouroutine = Flash(waitTime);
        StartCoroutine(fadeCouroutine);
    }

    IEnumerator Flash(float waitTime)
    {
        //increases alpha
        for (; alpha < 1f; alpha += flashSpeed)
        {
            yield return null;
        }

        //starts the next coroutine
        yield return new WaitForSeconds(waitTime);

        for (; alpha > 0f; alpha -= fadeSpeed)
        {
            yield return null;
        }
    }
}