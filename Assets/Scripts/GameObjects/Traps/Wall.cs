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
            Debug.Log("isColliding Wall)");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isServer) return;

        if (collision.gameObject.tag == "Relic")
        {
            isCollidingWithRelic = true;
            timer = 0f;
        } 

        if (collision.gameObject.tag == "Player")
        {
            GetComponent<Renderer>().enabled = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (!isServer) return;

        if (collision.gameObject.tag == "Relic")
        {
            isCollidingWithRelic = false;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (!isServer) return;

        if (collision.gameObject.tag == "Relic")
        {
            Debug.Log("Colliding with wall!");
            timer += Time.deltaTime;
            if (timer > TIME_TO_DESTROY)
            {
                Network.Destroy(gameObject);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Relic")
        {
            Debug.Log("Colliding relic");
        }
    }
}
