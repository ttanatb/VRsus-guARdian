using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// A sample of the relic that is placed in the training area
/// 
/// Author: Tanat Boozayaangool
/// </summary>
public class SampleRelic : MonoBehaviour
{
    private bool stolen = false;

    [Tooltip("The renderer of the relic itself")]
    public Renderer relic;

    private ParticleSystem[] particles;
    private Light spotLight;

    private void Start()
    {
        spotLight = GetComponentInChildren<Light>();
        particles = GetComponentsInChildren<ParticleSystem>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (stolen) return;

        //fakes the collection
        if (other.tag == "Player")
        {
            spotLight.enabled = false;
            relic.enabled = false;

            foreach (ParticleSystem p in particles)
                p.Stop();

            stolen = true;

            if (!transform.parent.GetComponent<NetworkIdentity>().isServer)
                CanvasManager.Instance.SetMessage("You stole a practice relic!");
        }
    }

    public void ResetPractice()
    {
        relic.enabled = true;
        spotLight.enabled = true;
        stolen = false;

        foreach (ParticleSystem p in particles)
            p.Play();
    }
}
