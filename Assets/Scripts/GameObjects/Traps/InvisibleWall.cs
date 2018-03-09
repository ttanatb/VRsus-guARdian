using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// A wall that becomes visible once the player collides with it.
/// 
/// Author: Tanat Boozayaangool
/// </summary>
public class InvisibleWall : TrapDefense
{
    //fields
    bool isActive = false;

    public override string TrapName { get { return "Rising Wall"; } }

    private Animator animator;
    private ParticleSystem particles;
    private MeshRenderer mRenderer;

    private BoxCollider triggerCollider;
    private BoxCollider realCollider;

    // Use this for initialization
    void Start()
    {
        animator = GetComponent<Animator>();
        particles = GetComponentInChildren<ParticleSystem>();

        if (!isServer)
        {
            mRenderer = GetComponentInChildren<MeshRenderer>();
            mRenderer.enabled = false;
        }
        else
        {
            BoxCollider[] colliders = GetComponents<BoxCollider>();
            foreach (BoxCollider c in colliders)
            {
                if (c.isTrigger)
                    triggerCollider = c;
                else realCollider = c;
            }

            triggerCollider.enabled = false;
            Vector3 center = realCollider.center;
            center.y *= -1;
            realCollider.center = center;
        }
    }

    // Collision detection
    private void OnTriggerEnter(Collider other)
    {
        if (!isServer || isActive) return;

        if (other.gameObject.tag == "Player")
        {
            isActive = true;
            RpcEnable();
        }
    }

    /// <summary>
    /// Enables the trap for the clients (VR Player)
    /// </summary>
    [ClientRpc]
    private void RpcEnable()
    {
        if (!isServer)
            mRenderer.enabled = true;

        animator.SetTrigger("Trigger");
        particles.Play();
        gameObject.layer = 1 << 0;
    }

    [Server]
    public override void TransitionToPlayPhase()
    {
        triggerCollider.enabled = true;
        Vector3 center = realCollider.center;
        center.y *= -1;
        realCollider.center = center;
    }
}
