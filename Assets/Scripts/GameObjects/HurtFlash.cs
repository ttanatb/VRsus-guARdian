using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This script controls the red color that appears on the 
/// screen represent getting hurt
/// 
/// Author: Tanat Boozayaangool
/// </summary>
public class HurtFlash : MonoBehaviour
{
    #region Fields
    [Tooltip("The speed in which the red color fades in")]
    [Range(0f, 1f)]
    public float flashSpeed;

    [Tooltip("The speed in which the red color fades iway")]
    [Range(0f, 1f)]
    public float fadeSpeed;

    [Tooltip("The amount of time to wait until fading away")]
    public float waitTime = 3f;

    private Image image;
    private float alpha = 0;
    private IEnumerator fadeCouroutine;
    #endregion

    #region Init Logic
    //start
    void Start()
    {
        image = GetComponent<Image>();
    }
    #endregion

    #region Life Cycle
    // Update is called once per frame
    void Update()
    {
        //get current color
        Color c = image.color;

        //clamp the alpha
        if (alpha > 1f)
            alpha = 1f;
        else if (alpha < 0f)
            alpha = 0f;

        //set the alpha
        c.a = alpha;
        image.color = c;
    }

    /// <summary>
    /// Flashes a red color
    /// </summary>
    public void FlashRed()
    {
        if (fadeCouroutine != null)
            StopCoroutine(fadeCouroutine);

        fadeCouroutine = Flash(waitTime);
        StartCoroutine(fadeCouroutine);
    }

    /// <summary>
    /// Coroutine to fade red in, wait, and fade it out
    /// </summary>
    /// <param name="waitTime">Time to wait</param>
    IEnumerator Flash(float waitTime)
    {
        //increases alpha
        for (; alpha < 1f; alpha += flashSpeed)
            yield return null;

        //starts the next coroutine
        yield return new WaitForSeconds(waitTime);

        for (; alpha > 0f; alpha -= fadeSpeed)
            yield return null;
    }
    #endregion
}