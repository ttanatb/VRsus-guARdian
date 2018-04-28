using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HookshotUI : MonoBehaviour
{
    private Transform player;
    [SerializeField]
    private int layerMask;
    [SerializeField]
    private Color canTargetColor;
    [SerializeField]
    private Color cannotTargetColor;
    private Image image;


    float maxDist;

    private void Start()
    {
        image = GetComponent<Image>();
    }

    public void Init(Transform transform, int laserMask, float distance)
    {
        player = transform;
        layerMask = laserMask;
        maxDist = distance;
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit canTargetHit;
        RaycastHit cannotTargetHit;
        if (!Physics.Raycast(player.position, player.forward, out canTargetHit, maxDist, layerMask)) //does not raycast to anything hittable
        {
            image.color = cannotTargetColor;
        }
        else
        {
            if (Physics.Raycast(player.position, player.forward, out cannotTargetHit, maxDist, ~layerMask)) //hit something it can't target
            {
                //calculate distances
                if ((player.position - canTargetHit.point).sqrMagnitude < (player.position - cannotTargetHit.point).sqrMagnitude)
                {
                    image.color = canTargetColor;
                }
                else
                {
                    image.color = cannotTargetColor;
                }
            }
            else
            {
                //hit target, but does not hit cannot target
                image.color = canTargetColor;
            }
        }
    }
}
