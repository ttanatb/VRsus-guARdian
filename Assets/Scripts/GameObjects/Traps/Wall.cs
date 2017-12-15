using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Wall : TrapDefense
{
    //private Vector2 scale = Vector2.one;
    //private Material m;

    private bool isCollidingWithRelic = false;
    private float timer = 0f;
    private float TIME_TO_DESTROY = 3f;

    [SyncVar]
    bool isActive = false;

    public override string TrapName
    {
        get
        {
            return "Invisible Wall";
        }
    }

    // Use this for initialization
    void Start()
    {
       //m = GetComponent<Renderer>().material;
#if !UNITY_IOS
        GetComponent<Renderer>().enabled = false;
#endif
    }

    // Update is called once per frame
    void Update()
    {
        //scale = new Vector2(transform.localScale.x, transform.localScale.z);
        //if (!m)
        //    m = GetComponent<Renderer>().material;
        //m.SetTextureScale("_MainTex", scale / 0.75f);


        if (isCollidingWithRelic)
        {
            //Debug.Log("isColliding Wall)");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isServer || isActive) return;

        if (collision.gameObject.tag == "Relic")
        {
            isCollidingWithRelic = true;
            timer = 0f;
        } 

        if (collision.gameObject.tag == "Player")
        {
            GetComponent<Renderer>().enabled = true;
            isActive = true;
            RpcEnable();
            GetComponent<Rigidbody>().isKinematic = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isServer || isActive) return;

        if (other.gameObject.tag == "Player")
        {
            GetComponent<Renderer>().enabled = true;
            isActive = true;
            RpcEnable();
            GetComponent<Rigidbody>().isKinematic = true;
        }
    }

    [ClientRpc]
    private void RpcEnable()
    {
        GetComponent<Renderer>().enabled = true;
        GetComponent<Rigidbody>().isKinematic = true;
    }
}
