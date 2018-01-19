using UnityEngine;

/// <summary>
/// A sample of the wall trap that is placed in the training area
/// 
/// Author: Tanat Boozayaangool
/// </summary>
public class SampleWall : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
        GetComponent<Renderer>().enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        //makes the wall visible on contact
        if (other.tag == "Player")
            GetComponent<Renderer>().enabled = true;
    }
}
