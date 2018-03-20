using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PracticeEntrance : MonoBehaviour
{
    [SerializeField]
    private Vector3 spawnPos;

    private ParticleSystem pSystem;
    private Light attachedLight;

    [SerializeField]
    private bool activated = false;

    private PraticeArea practiceArea;

    // Use this for initialization
    void Start()
    {
        pSystem = GetComponentInChildren<ParticleSystem>();
        attachedLight = GetComponentInChildren<Light>();
        attachedLight.enabled = false;
    }

    public void Activate(Vector3 spawnPos, PraticeArea practiceArea)
    {
        this.spawnPos = spawnPos;
        this.practiceArea = practiceArea;
        pSystem.Play();
        attachedLight.enabled = true;
        activated = true;
    }

    public void Deactivate()
    {
        spawnPos = transform.position;
        pSystem.Stop();
        attachedLight.enabled = false;
        activated = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!activated) return;

        Debug.Log(other);
        if (other.CompareTag("Player"))
        {
            VRTransition transition = other.transform.parent.GetComponent<VRTransition>();
            if (transition)
            {
                transition.SpawnInPos(spawnPos);
                practiceArea.ResetPracticeArea();
            }
        }
    }
}
