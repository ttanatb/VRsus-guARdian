using UnityEngine;

/// <summary>
/// Helper script to play the laser particle system
/// 
/// Author: Tanat Boozayaangool
/// </summary>
public class LaserParticle : SingletonMonoBehaviour<LaserParticle>
{
    private ParticleSystem pSystem;
    private ParticleSystem[] allParticles;

    protected override void Awake()
    {
        base.Awake();
        pSystem = GetComponentInChildren<ParticleSystem>();
    }

    private void Start()
    {
        allParticles = GetComponentsInChildren<ParticleSystem>();
    }

    public void SetDuration(float duration)
    {
        foreach(ParticleSystem p in allParticles)
        {
            ParticleSystem.MainModule m = p.main;
            m.duration = duration;
        }

        ParticleSystem.MainModule module = pSystem.main;
        module.startLifetime = duration; //new ParticleSystem.MinMaxCurve()
    }

    /// <summary>
    /// Stops the particle system if it is playing
    /// </summary>
    public void Stop()
    {
        if (pSystem.isPlaying)
            pSystem.Stop();
    }

    /// <summary>
    /// Plays the particle system if it is not playing
    /// </summary>
    public void Play()
    {
        if (!pSystem.isPlaying)
            pSystem.Play();
    }
}
