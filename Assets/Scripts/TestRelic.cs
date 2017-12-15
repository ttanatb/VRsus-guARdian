using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestRelic : MonoBehaviour
{
    public Renderer relic;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            relic.enabled = true;
        }
    }
}
