using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardAnim : MonoBehaviour
{

    Animator anim;
    Vector3 previousPos;
    public float movementThreshold = 0.3f;
    public float runThreshold = 1f;

    [SerializeField]
    private float movementThresholdSqr;

    [SerializeField]
    private float runThresholdSqr;

    [SerializeField]
    private float dist;

    // Use this for initialization
    void Start()
    {
        anim = GetComponent<Animator>();
        anim.SetBool("IsPausing", true);

        movementThresholdSqr = Mathf.Pow(movementThreshold * transform.localScale.z, 2f);
        runThresholdSqr = Mathf.Pow(runThreshold * transform.localScale.z, 2f);

        previousPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        dist = (transform.position - previousPos).sqrMagnitude;
        if (dist > runThresholdSqr)
        {
            anim.SetBool("IsChasing", true);
            anim.SetBool("IsPausing", false);
        }
        else if (dist > movementThresholdSqr)
        {
            anim.SetBool("IsChasing", false);
            anim.SetBool("IsPausing", false);
        }
        else
        {
            anim.SetBool("IsChasing", false);
            anim.SetBool("IsPausing", true);
        }

        previousPos = transform.position;

    }

    //IEnumerator FadeIn()
    //{
    //  for (;;)
    //  {
    //      Color c = skinnedR.material.color;
    //      c.a *= 0.91f;
    //      skinnedR.material.color = c;
    //      if (c.a < 0.01f)
    //      {
    //          skinnedR.enabled = false;
    //          yield return null;
    //      }
    //      else yield return new WaitForSeconds(0.01f);
    //  }
    //}
}
