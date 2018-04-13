using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Handles combat for the AR player (shooting lasers)
/// 
/// Author: Tanat Boozayaangool
/// </summary>
public class ARCombat : PlayerComponent
{
    #region Fields
    //public fields
    [Tooltip("The layermask in which lasers should interact with")]
    public LayerMask laserLayerMask;

    [Tooltip("The furthest distance in which lasers can reach")]
    public float layerMaxDist = 4f;

    [Tooltip("Duration in which lasers last")]
    public float laserDuration = 1f;

    [Tooltip("Cooldown before firing lasers again")]
    public float laserCoolDown = 2f;

    public float windUpDuration = 0.1f;

    //networked data
    [SyncVar]
    private Vector3 laserPoint;

    [SyncVar]
    private bool isShootingLaser;

    private bool canShoot = false;
    private float laserTimer = 0f;

    //pointers
    private LaserParticle laserParticle;
    private Transform avatar;

    private bool isShootingEnabled = false;

    public LaserBeamParticle laserBeam;
    public Light flashLight;

    private float spotLightAngle;

    /// <summary>
    /// Changes if the character can shoot
    /// </summary>
    public bool IsShootingEnabled
    {
        set
        {
            laserTimer = 0f;
            CanvasManager.Instance.SetCrossHairUI(value);
            isShootingEnabled = value;
        }
    }
    #endregion

    #region Init & Destructor
    // Use this for initialization
    void Start()
    {
        InitObj();
        laserParticle = LaserParticle.Instance;
        laserParticle.SetDuration(laserDuration);
        laserBeam.SetDuration(laserDuration);
        avatar = player.avatar.transform;
        spotLightAngle = flashLight.spotAngle;
        laserBeam.SetOffset(-Vector3.up * 0.3f);
    }

    public override void OnStartLocalPlayer()
    {
        flashLight.enabled = false;
    }

    protected override void InitObj() { }
    #endregion

    #region Life Cycle
    // Update is called once per frame
    void Update()
    {
        //locally updates the display of the laser
        if (isShootingLaser)
            DisplayLaser();

        //updates where the laser should be
        UpdateLaser();

        if (!isLocalPlayer)
            return;

        //checks input and if laser should be shot
        if (isShootingEnabled && canShoot && CheckTap() && !Utility.IsPointerOverUIObject())
            RpcFire();
    }

    /// <summary>
    /// Updates if the laser display locally
    /// </summary>
    private void DisplayLaser()
    {
        //if laser should be seen
        //updates where the particles should be
        laserBeam.UpdateRange(laserPoint);
        laserParticle.transform.position = laserPoint;

        //updates where the other laser should be
        //if (isLocalPlayer)
        //{
        //    //laserBeam.SetPosition(0, avatar.position - avatar.up * 0.03f);
        //    laserBeam.SetPosition(laserPoint);
        //}
        //else
        //{
        //    //laserBeam.SetPosition(0, avatar.position);
        //}
    }

    /// <summary>
    /// Raycasts and updates where the other end of the laser should be
    /// </summary>
    private void UpdateLaser()
    {
        //if the laser is being shot
        if (isShootingLaser)
        {
            laserTimer += Time.deltaTime;
            if (laserTimer < windUpDuration)
                flashLight.spotAngle = Mathf.Lerp(spotLightAngle, 0, laserTimer / windUpDuration);

            if (!isLocalPlayer) return;

            //updates timer
            if (laserTimer > laserDuration)
            {
                laserTimer = 0f;
                isShootingLaser = false;
            }

            if (laserTimer > windUpDuration)
            {
                //raycasts
                RaycastHit hit;

                //puts the laser where it 'hits'
                if (Physics.Raycast(avatar.position, -avatar.forward, out hit, layerMaxDist, laserLayerMask))
                {
                    laserPoint = hit.point;

                    if (hit.transform.tag == "Player")
                    {
                        VRCombat combat = hit.transform.GetComponent<VRCombat>();
                        combat.TakeDamage();
                    }
                }
            }
            //puts it at the furthest distance
            else laserPoint = avatar.position + -avatar.forward * layerMaxDist;
        }

        //manages cool down if laser isn't being shot
        else
        {
            if (laserTimer < laserCoolDown)
            {
                laserTimer += Time.deltaTime;
                flashLight.spotAngle = Mathf.Lerp(0, spotLightAngle, laserTimer / laserCoolDown);
            }
            else if (isLocalPlayer)
            {
                canShoot = true;
            }
        }
    }

    /// <summary>
    /// Checks if there is a tap input
    /// 
    /// To-do: Extend an input class and put this there
    /// </summary>
    /// <returns>Whether or not ther was a tap</returns>
    private bool CheckTap()
    {
#if UNITY_IOS
        if (Input.touchCount > 0)
        {
            foreach (Touch t in Input.touches)
            {
                if (t.phase != TouchPhase.Canceled)
                {
                    return true;
                }
            }
        }
        return false;
#else
        return Input.GetMouseButton(0);
#endif
    }

    /// <summary>
    /// 'Fires' out the laser
    /// </summary>
    [ClientRpc]
    private void RpcFire()
    {
        isShootingLaser = true;
        canShoot = false;
        laserTimer = 0f;
        laserBeam.Play();
        laserParticle.Play();
    }
    #endregion
}