using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fader : MonoBehaviour
{
    public Material transparentMat;
    public float fadeSpeed = 0.1f;
    private Material defaultMat;
    public Renderer charRenderer;

    private Color transparentC;
    private Color solidColor;
    private float timer;
    public TrailRenderer trailRenderer;

    private VRCombat combat;
    // Use this for initialization
    void Start()
    {
        if (!charRenderer)
            charRenderer = GetComponentInChildren<Renderer>();
        defaultMat = charRenderer.material;
        transparentC = new Color(0, 0, 0, 0);
        solidColor = new Color(0, 0, 0, 0.95f);
        timer = 0f;
        combat = GetComponentInParent<VRCombat>();
    }

    public void Fade(float fadeTime)
    {
        charRenderer.material = transparentMat;
        timer = 0f;
        IEnumerator fade = Fade(fadeTime, fadeSpeed);
        trailRenderer.enabled = false;
        StartCoroutine(fade);
    }

    private IEnumerator Fade(float fadeTime, float fadeSpeed)
    {
        while (timer < 1f)
        {
            timer += fadeSpeed;
            charRenderer.material.SetColor("_Color", Color.Lerp(solidColor, transparentC, timer));
            yield return new WaitForEndOfFrame();
        }

        //waits before fading in
        yield return new WaitForSeconds(fadeTime);

        while (timer < 2f)
        {
            timer += fadeSpeed;
            charRenderer.material.SetColor("_Color", Color.Lerp(transparentC, solidColor, timer - 1f));
            yield return new WaitForEndOfFrame();
        }

        //swap mat
        charRenderer.material = defaultMat;

        trailRenderer.enabled = true;

        //set invul
        combat.IsInvulnerable = false;

        yield return null;
    }
}
