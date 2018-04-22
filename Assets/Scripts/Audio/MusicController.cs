using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicController : SingletonMonoBehaviour<MusicController>
{
    [SerializeField]
    private float[] speeds = { 0.95f, 0.975f, 1.0f };

    [SerializeField]
    private int level = 0;

    [SerializeField]
    private float speedChangeTime = 1.65f;

    private float timer = 0f;

    private AudioSource audioSrc;
    
    // Use this for initialization
    void Start()
    {
        audioSrc = GetComponent<AudioSource>();
        audioSrc.pitch = speeds[level];
    }

    public void IncrementSpeed()
    {
        level = Mathf.Clamp(level + 1, 0, speeds.Length - 1);
        timer = 0f;

        StartCoroutine(ChangePitch());
    }

    IEnumerator ChangePitch()
    {
        while(timer < speedChangeTime)
        {
            audioSrc.pitch = Mathf.Lerp(speeds[level - 1], speeds[level], timer / speedChangeTime);
            timer += Time.deltaTime;
            yield return null;
        }
    }
}
