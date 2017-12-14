using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class LaserParticle : MonoBehaviour
{
    ///private bool isPlaying = false;
    private ParticleSystem particles;

    // Use this for initialization
    void Start()
    {
        particles = GetComponent<ParticleSystem>();
        particles.Play();
    }

    public void Stop()
    {
        if (particles.isPlaying)
        {
            //Debug.Log("Stopping!");
            particles.Stop();
        } 
    }

    public void Play()
    {
        if (!particles.isPlaying)
        {
            //Debug.Log("Play!");
            particles.Play();
        }
    }
}
