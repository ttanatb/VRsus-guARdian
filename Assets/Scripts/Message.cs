using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Message : MonoBehaviour
{
    private Text text;
    public float timeToFade = 2f;
    public float fadeSpeed = 0.14f;

    private float timer = 0f;

    private bool isDisplay = false;
    private CanvasRenderer[] renderers;

    private IEnumerator fadeCoroutine;
    float alpha = 1f;

    // Use this for initialization
    void Start()
    {
        text = GetComponentInChildren<Text>();
        renderers = GetComponentsInChildren<CanvasRenderer>();
        fadeCoroutine = FadeOut();

        foreach (CanvasRenderer r in renderers)
        {
            r.SetAlpha(0f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isDisplay)
        {
            timer += Time.deltaTime;

            if (timer > timeToFade)
            {
                if (fadeCoroutine != null)
                    StopCoroutine(fadeCoroutine);

                fadeCoroutine = FadeOut();
                StartCoroutine(fadeCoroutine);
            }
        }
    }

    public void SetMessage(string msg)
    {
        timer = 0f;
        text.text = msg;
        isDisplay = true;
        alpha = 1f;
        foreach (CanvasRenderer r in renderers)
        {
            r.SetAlpha(1f);
        }
    }

    public void SetPermanentMessage(string msg)
    {
        timer = float.MinValue;
        text.text = msg;
        alpha = 1f;
        foreach (CanvasRenderer r in renderers)
        {
            r.SetAlpha(1f);
        }
    }

    public void ClearMsg()
    {
        timer = timeToFade;
        isDisplay = true;
    }

    IEnumerator FadeOut()
    {
        for(; alpha > 0f; alpha -= fadeSpeed)
        {
            alpha -= fadeSpeed;
            foreach (CanvasRenderer r in renderers)
            {
                r.SetAlpha(alpha);
            }
            yield return null;
        }

        foreach (CanvasRenderer r in renderers)
        {
            r.SetAlpha(0f);
        }

        isDisplay = false;
    }
}
