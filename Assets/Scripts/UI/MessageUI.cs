using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Class that manages the message UI locally
/// 
/// Author: Tanat Boozayaangool
/// </summary>
public class MessageUI : MonoBehaviour
{
    #region Fields
    //fading values
    public float timeToFade = 2f;
    public float fadeSpeed = 0.14f;
    private IEnumerator fadeCoroutine;

    private bool isDisplay = false;
    private float alpha = 1f;
    private float timer = 0f;

    private Text text;
    private CanvasRenderer[] renderers;
    #endregion

    #region Init Logic
    // Use this for initialization
    void Start()
    {
        text = GetComponentInChildren<Text>();
        renderers = GetComponentsInChildren<CanvasRenderer>();
        fadeCoroutine = FadeOut();

        foreach (CanvasRenderer r in renderers)
            r.SetAlpha(0f);
    }
    #endregion

    #region Life Cycle
    /// <summary>
    /// Updates the timer and fades out the message if needed
    /// </summary>
    void Update()
    {
        if (!isDisplay) return;

        timer += Time.deltaTime;

        if (timer > timeToFade)
        {
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);

            fadeCoroutine = FadeOut();
            StartCoroutine(fadeCoroutine);
        }
    }

    /// <summary>
    /// Coroutine to handle fading out
    /// </summary>
    private IEnumerator FadeOut()
    {
        //fades out incrementally
        for (; alpha > 0f; alpha -= fadeSpeed)
        {
            alpha -= fadeSpeed;
            foreach (CanvasRenderer r in renderers)
                r.SetAlpha(alpha);

            yield return null;
        }

        //finalizes the fade out
        isDisplay = false;
        foreach (CanvasRenderer r in renderers)
            r.SetAlpha(0f);
    }

    /// <summary>
    /// Sets a message to be displayed
    /// </summary>
    /// <param name="msg">Message to display</param>
    public void SetMessage(string msg)
    {
        timer = 0f;
        text.text = msg;
        isDisplay = true;
        alpha = 1f;
        foreach (CanvasRenderer r in renderers)
            r.SetAlpha(1f);
    }

    /// <summary>
    /// Sets a message to be displayed or a really long time
    /// </summary>
    /// <param name="msg">Message to display</param>
    public void SetPermanentMessage(string msg)
    {
        timer = float.MinValue;
        text.text = msg;
        alpha = 1f;
        foreach (CanvasRenderer r in renderers)
            r.SetAlpha(1f);
    }

    /// <summary>
    /// Clears out any message that is on screen
    /// </summary>
    public void ClearMsg()
    {
        isDisplay = false;
        foreach (CanvasRenderer r in renderers)
            r.SetAlpha(0f);
    }
    #endregion
}
