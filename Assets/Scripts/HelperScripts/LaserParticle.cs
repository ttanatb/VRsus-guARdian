using UnityEngine;

/// <summary>
/// Helper script to play the laser particle system
/// 
/// Author: Tanat Boozayaangool
/// </summary>
public class LaserParticle : MonoBehaviour
{
    private ParticleSystem particles;

    // Use this for initialization
    void Awake()
    {
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
