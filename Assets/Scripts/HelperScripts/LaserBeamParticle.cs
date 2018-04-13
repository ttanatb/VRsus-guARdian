using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserBeamParticle : MonoBehaviour
{
    private ParticleSystem pSystem;
    public ParticleSystem[] beamParticles;

    private Vector3 scale;

    // Use this for initialization
    void Start()
    {
        pSystem = GetComponent<ParticleSystem>();
        scale = Vector3.one;
    }

    public void Play()
    {
        pSystem.Play();
    }

    public void SetDuration(float duration)
    {
        ParticleSystem[] allParticles = GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem p in allParticles)
        {

            ParticleSystem.MainModule m = p.main;
            m.duration = duration;
        }

        foreach (ParticleSystem p in beamParticles)
        {
            ParticleSystem.MainModule m = p.main;
            m.startLifetime = duration;
        }
    }

    public void UpdateRange(Vector3 targetPos)
    {
        scale.z = (targetPos - transform.position).magnitude / 5.5f;
        foreach (ParticleSystem p in beamParticles)
            p.transform.localScale = scale;

        transform.localRotation = Quaternion.AngleAxis(-Vector3.Angle(targetPos - transform.position, transform.parent.forward), Vector3.right);
    }

    public void SetOffset(Vector3 offset)
    {
        transform.localPosition = offset;
    }
}
