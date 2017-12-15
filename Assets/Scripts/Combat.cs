using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.EventSystems;
#if UNITY_IOS
using UnityEngine.iOS;
#endif

public class Combat : NetworkBehaviour
{
    public const int maxHealth = 3;

    [SyncVar]
    public int health = maxHealth;

    public GameObject springPadPrefab;

    public GameObject bulletPrefab;
    public float bulletSpeed = 1f;
    public float bulletTimer = 2f;

    public Text playerHealth;
    public Text enemyHealth;

    public GameObject HurtScreenPrefab;

    private HurtFlash[] hurtFlashes;
    private int hurtFlashCount = 7;
    private int hurtFlashIndex = 0;

    private Player player;
    public Transform canvas;

    private Transform avatar;
    private Vector3 prevPos;

    private int prevHealth = maxHealth;

    public GameObject healthBarPrefab;
    public GameObject healthBarUIPrefab;

    private HealthBar healthBar;
    private HealthBarUI healthBarUI;

    private bool canShoot = false;

    private GameManager manager;

    //shooting laser
    private LineRenderer lineRenderer;
    public LayerMask laserLayerMask;
    public float layerMaxDist;

    private LaserParticle laserParticle;

    [SyncVar]
    private Vector3 laserPoint;

    [SyncVar]
    private bool isShootingLaser;

    private float laserTimer = 0f;
    private bool isReadyToShoot = false;
    public float laserDuration = 2f;
    public float laserCoolDown = 1f;

    public bool CanShoot
    {
        set
        {
            crosshairObj.SetActive(value);
            canShoot = value;
        }
    }

    [SyncVar]
    private bool isInvulnerable = false;
    private float invulTimer = 0f;

    public const float MAX_INVUL_TIME = 1.5f;

    [SyncVar]
    private int relicCount = 0;

    public GameObject crosshairObj;
    private LayerMask shootLayer;
    LineRenderer laser;// = GetComponent<LineRenderer>();

    public bool IsInvulnerable
    {
        get { return isInvulnerable; }
    }

    private void Awake()
    {
        if (!canvas)
            canvas = GameObject.Find("Canvas").transform;
    }

    private void Start()
    {
        player = GetComponent<Player>();

        laser = GetComponent<LineRenderer>();
        laserParticle = FindObjectOfType<LaserParticle>();
        if (player.PlayerType == PlayerType.AR)
        {
            avatar = player.ARAvatar.transform;
            laser.enabled = true;
        }
        else
        {
            avatar = player.VRAvatar.transform;
        }

        prevPos = avatar.position;
        shootLayer = crosshairObj.layer;

        if (isServer)
            manager = FindObjectOfType<GameManager>();
    }

    public void InitHealthBar()
    {
        if (player.PlayerType == PlayerType.AR)
            return;

        CmdCreateHealthBar();

        player.VRAvatar.GetComponent<Collider>().enabled = true;
        healthBarUI = Instantiate(healthBarUIPrefab, canvas).GetComponent<HealthBarUI>();
        healthBarUI.Init(this);
    }

    [Command]
    private void CmdCreateHealthBar()
    {
        if (isLocalPlayer)
        {
            return;
        }

        //maybe this won't work?
        healthBar = Instantiate(healthBarPrefab).GetComponent<HealthBar>();
        healthBar.Init(this, player.PlayerType, avatar);
        GetComponent<Player>().EnableVRPlayerRenderers();
    }

    public override void OnStartLocalPlayer()
    {
        hurtFlashes = new HurtFlash[hurtFlashCount];
        for (int i = 0; i < hurtFlashCount; i++)
        {
            hurtFlashes[i] = Instantiate(HurtScreenPrefab, canvas).GetComponent<HurtFlash>();
        }
    }

    private void OnDestroy()
    {
        if (hurtFlashes != null)
        {
            for (int i = 0; i < hurtFlashCount; i++)
            {
                if (hurtFlashes[i])
                    Destroy(hurtFlashes[i].gameObject);
            }
        }

        if (healthBar)
            Destroy(healthBar.gameObject);

        if (healthBarUI)
            Destroy(healthBarUI.gameObject);

        if (!lineRenderer)
            lineRenderer = GetComponent<LineRenderer>();

        if (lineRenderer)
            Destroy(lineRenderer);
    }

    // Update is called once per frame
    void Update()
    {
        if (player.PlayerType == PlayerType.VR)
            avatar.forward = transform.forward;

        if (isServer && isInvulnerable)
        {
            //if (invulTimer > MAX_INVUL_TIME)
            //{
            //    isInvulnerable = false;
            //    foreach (Renderer r in player.VRAvatar.GetComponentsInChildren<Renderer>())
            //    {
            //        r.enabled = true;
            //    }
            //    foreach (Renderer r in healthBar.GetComponentsInChildren<Renderer>())
            //    {
            //        r.enabled = true;
            //    }
            //}
            //else invulTimer += Time.deltaTime;
        }


        if (player.PlayerType == PlayerType.AR)
        {
            if (isShootingLaser)
            {
                laserParticle.transform.position = laserPoint;
                //laserParticle.transform.up = Vector3.up;
                laserParticle.Play();

                if (isLocalPlayer)
                {
                    laser.SetPosition(0, avatar.position - avatar.up * 0.02f);
                    laser.SetPosition(1, laserPoint);
                }
                else
                {
                    laser.SetPosition(0, avatar.position);
                    laser.SetPosition(1, laserPoint);
                }
            }
            else
            {
                laserParticle.Stop();
                laser.SetPosition(0, avatar.position);
                laser.SetPosition(1, avatar.position);
            }
        }

        if (!isLocalPlayer)
            return;

        if (isShootingLaser)
        {
            RaycastHit hit;
            if (Physics.Raycast(avatar.position, avatar.forward, out hit, layerMaxDist, laserLayerMask))
            {
                //print(hit.transform.gameObject.name);
                laserPoint = hit.point;
                if (hit.transform.tag == "Player")
                    hit.transform.GetComponent<CameraAvatar>().rootPlayer.GetComponent<Combat>().TakeDamage();

            }
            else
            {
                laserPoint = avatar.position + avatar.forward * layerMaxDist;
            }

            laserTimer += Time.deltaTime;
            if (laserTimer > laserDuration)
            {
                laserTimer = 0f;
                isShootingLaser = false;
            }
        }
        else
        {
            laserTimer += Time.deltaTime;
            if (laserTimer > laserCoolDown)
            {
                isReadyToShoot = true;
            }
        }


        if (player.PlayerType == PlayerType.AR && Utility.IsPointerOverUIObject())
            return;

        if (isReadyToShoot)
        {
            if (CheckTap())
            {
                CmdFire(avatar.position, avatar.forward);
            }
        }

        if (prevHealth != health)
        {
            hurtFlashes[hurtFlashIndex].FlashRed();
            hurtFlashIndex++;
            if (hurtFlashIndex > hurtFlashCount - 1)
                hurtFlashIndex = 0;
        }

        prevHealth = health;
        prevPos = avatar.position;
    }

    bool CheckTap()
    {
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
    }

    public UnityAction GetActionToSwitchToPlacingMode()
    {
        UnityAction action = () =>
        {
        };

        return action;
    }

    [Command]
    void CmdFire(Vector3 pos, Vector3 forward)
    {
        if (!canShoot)
            return;

        //laser.enabled = true;
        isShootingLaser = true;
        isReadyToShoot = false;
        laserTimer = 0f;

        //GameObject bulletObj = null;
        //if (player.PlayerType == PlayerType.AR)
        //{
        //    bulletObj = Instantiate(bulletPrefab,
        //    avatar.position + avatar.localScale.z * avatar.forward,
        //    Quaternion.identity);
        //    bulletObj.GetComponent<Rigidbody>().velocity = avatar.forward * bulletSpeed;// + (avatar.position - prevPos);		
        //}
        //else
        //{
        //    bulletObj = Instantiate(bulletPrefab,
        //    transform.position + avatar.localScale.z * transform.forward,
        //    Quaternion.identity);
        //    bulletObj.GetComponent<Rigidbody>().velocity = transform.forward * bulletSpeed;// + (avatar.position - prevPos);		

        //}
        ////bulletObj = Instantiate(bulletPrefab, pos, Quaternion.identity);
        ////bulletObj.GetComponent<Rigidbody>().velocity = forward * bulletSpeed;
        //NetworkServer.Spawn(bulletObj);
        //bulletObj.GetComponent<Bullet>().Init(player.PlayerType, isLocalPlayer);
        //Destroy(bulletObj, bulletTimer);
    }

    [Command]
    void CmdCreateJumpPad(Vector3 position)
    {
        GameObject springPad = Instantiate(springPadPrefab, position, Quaternion.identity);
        NetworkServer.Spawn(springPad);
    }


    [Server]
    public void TakeDamage()
    {
        if (!isServer || isInvulnerable)
            return;

        health--;

        if (health < 1)
        {
            RpcDie();
        }
        else
        {
            isInvulnerable = true;
            IEnumerator flash = Flash(1.75f);
            StartCoroutine(flash);
            //invulTimer = 0f;
            //foreach(Renderer r in player.VRAvatar.GetComponentsInChildren<Renderer>())
            //{
            //    r.enabled = false;
            //}
            //foreach(Renderer r in healthBar.GetComponentsInChildren<Renderer>())
            //{
            //    r.enabled = false;
            //}
        }
    }

    [ClientRpc]
    void RpcDie()
    {
        if (!manager)
        {
            manager = FindObjectOfType<GameManager>();
        }

        manager.SetPhaseTo(GamePhase.Over);
        canShoot = false;

        CanvasManager.Instance.SetPermanentMessage("AR player wins!");
    }

    [ClientRpc]
    public void RpcRespawn()
    {
        transform.position = new Vector3(0.759f, 1005f, -0.659f);
        CanvasManager.Instance.ClearMsg();
        relicCount = 0;
        health = 3;
    }

    float alpha = 1f;
    float fadeSpeed = 0.2f;

    IEnumerator Flash(float waitTime)
    {
        //increases alpha
        //for (; alpha > 0f; alpha -= fadeSpeed)
        //{
        //    foreach (Renderer r in player.VRAvatar.GetComponentsInChildren<Renderer>())
        //    {
        //        Color c = r.material.color;
        //        c.a = alpha;
        //        r.material.color = c;
        //    }
        //    foreach (Renderer r in healthBar.GetComponentsInChildren<Renderer>())
        //    {
        //        Color c = r.material.color;
        //        c.a = alpha;
        //        r.material.color = c;
        //    }
        //    yield return null;
        //}
        healthBar.GetComponent<Renderer>().enabled = false;

        foreach (Renderer r in player.VRAvatar.GetComponentsInChildren<Renderer>())
        {
            r.enabled = false;
        }
        foreach (Renderer r in healthBar.GetComponentsInChildren<Renderer>())
        {
            r.enabled = false;
        }

        //starts the next coroutine
        yield return new WaitForSeconds(waitTime);

        //for (; alpha < 1f; alpha += fadeSpeed)
        //{
        //    foreach (Renderer r in player.VRAvatar.GetComponentsInChildren<Renderer>())
        //    {
        //        Color c = r.material.color;
        //        c.a = alpha;
        //        r.material.color = c;
        //    }
        //    foreach (Renderer r in healthBar.GetComponentsInChildren<Renderer>())
        //    {
        //        Color c = r.material.color;
        //        c.a = alpha;
        //        r.material.color = c;
        //    }
        //    yield return null;
        //}
        healthBar.GetComponent<Renderer>().enabled = true;

        foreach (Renderer r in player.VRAvatar.GetComponentsInChildren<Renderer>())
        {
            r.enabled = true;
        }
        foreach (Renderer r in healthBar.GetComponentsInChildren<Renderer>())
        {
            r.enabled = true;
        }

        isInvulnerable = false;
    }

    public void GainRelic()
    {
        relicCount += 1;
    }

    public int GetRelicCount()
    {
        return relicCount;
    }
}
