using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SpawnReelCamera : MonoBehaviour
{
    public GameObject cameraPrefab;

    // Use this for initialization
    void Start()
    {
        GameObject obj = Instantiate(cameraPrefab);
        obj.GetComponent<FollowCam>().followingObj = gameObject;

        GameObject.Find("Canvas").SetActive(false);
        GameObject.Find("Networking Manager").SetActive(false);
    }
}
