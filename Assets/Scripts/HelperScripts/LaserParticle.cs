using UnityEngine;

/// <summary>
/// Helper script to play the laser particle system
/// 
/// Author: Tanat Boozayaangool
/// </summary>
public class LaserParticle : SingletonMonoBehaviour<LaserParticle>
{
    private ParticleSystem particles;

    protected override void Awake()
    {
        base.Awake();
        particles = GetComponent<ParticleSystem>();
    }

    /// <summary>
    /// Stops the particle system if it is playing
    /// </summary>
    public void Stop()
    {
        if (particles.isPlaying)
            particles.Stop();
    }

    /// <summary>
    /// Plays the particle system if it is not playing
    /// </summary>
    public void Play()
    {
        if (!particles.isPlaying)
            particles.Play();
    }
}
