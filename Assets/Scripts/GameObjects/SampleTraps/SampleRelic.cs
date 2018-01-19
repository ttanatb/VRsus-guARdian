using UnityEngine;

/// <summary>
/// A sample of the relic that is placed in the training area
/// 
/// Author: Tanat Boozayaangool
/// </summary>
public class SampleRelic : MonoBehaviour
{
    [Tooltip("The renderer of the relic itself")]
    public Renderer relic;

    private void OnTriggerEnter(Collider other)
    {
        //fakes the collection
        if (other.tag == "Player")
            relic.enabled = false;
    }
}
