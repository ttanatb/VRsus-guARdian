using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserParticle : MonoBehaviour
{
    private bool isPlaying = false;
    private ParticleSystem particles;

    // Use this for initialization
    void Start()
    {
        particles = GetComponent<ParticleSystem>();
    }

    public void Stop()
    {
        if (isPlaying)
        {
            particles.Stop();
            isPlaying = false;
        }
    }

    public void Play()
    {
        if (!isPlaying)
        {
            particles.Play();
            isPlaying = true;
        }
    }
}
