using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Deals with health, invulnerability, and relic count
/// 
/// Author: Tanat Boozayaangool
/// </summary>
public class VRCombat : Combat
{
    #region Fields & Getter
    //reference to components
    private ARSetUp arSetUp;
    private Transform avatar;

    //health stuff
    public const int maxHealth = 3;

    [SyncVar]
    public int health = maxHealth;

    public GameObject healthBarPrefab;
    public GameObject healthBarUIPrefab;

    private Transform canvas;
    private HealthBar healthBar;
    private HealthBarUI healthBarUI;

    //invulnerability
    public const float MAX_INVUL_TIME = 1.5f;

    [SyncVar]
    private bool isInvulnerable = false;

    //visual feedback for getting hurt
    public GameObject HurtScreenPrefab;
    private HurtFlash[] hurtFlashes;
    private int hurtFlashCount = 7;
    private int hurtFlashIndex = 0;

    //relic stuff
    [SyncVar]
    private int relicCount = 0;

    public bool IsInvulnerable { get { return isInvulnerable; } }
    #endregion

    #region Init & Destruction
    public override void OnStartLocalPlayer()
    {
        //visual indicators for getting hurt
        hurtFlashes = new HurtFlash[hurtFlashCount];
        for (int i = 0; i < hurtFlashCount; i++)
            hurtFlashes[i] = Instantiate(HurtScreenPrefab, canvas).GetComponent<HurtFlash>();

        //UI for local player
        healthBarUI = Instantiate(healthBarUIPrefab, canvas).GetComponent<HealthBarUI>();
        healthBarUI.Init(this);
    }

    private void Start()
    {
        //
        canvas = CanvasManager.Instance.transform;
        avatar = player.VRAvatar.transform;

        if (!isLocalPlayer)
        {
            //physical health bar
            healthBar = Instantiate(healthBarPrefab).GetComponent<HealthBar>();
            healthBar.Init(this, playerType, avatar);
        }
    }

    protected override void InitObj() { }

    private void OnDestroy()
    {
        if (isLocalPlayer)
        {
            if (hurtFlashes != null)
            {
                for (int i = 0; i < hurtFlashCount; i++)
                {
                    if (hurtFlashes[i])
                        Destroy(hurtFlashes[i].gameObject);
                }
            }
            if (healthBarUI)
                Destroy(healthBarUI.gameObject);
        }
        else
        {
            if (healthBar)
                Destroy(healthBar.gameObject);
        }
    }
    #endregion

    void Update()
    {
        if (!isLocalPlayer)
            return;
    }

    [Server]
    public override void TakeDamage()
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
            RpcFlashRed();
            isInvulnerable = true;
            IEnumerator flash = Flash(MAX_INVUL_TIME);
            StartCoroutine(flash);
        }
    }

    [ClientRpc]
    void RpcFlashRed()
    {
        if (!isLocalPlayer) return;

        hurtFlashes[hurtFlashIndex].FlashRed();
        hurtFlashIndex++;
        if (hurtFlashIndex > hurtFlashCount - 1)
            hurtFlashIndex = 0;
    }

    [ClientRpc]
    void RpcDie()
    {
        if (!arSetUp)
            arSetUp = FindObjectOfType<ARSetUp>();

        arSetUp.SetPhaseTo(GamePhase.Over);
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

    IEnumerator Flash(float waitTime)
    {
        //turn this to a fading sort of thing?
        foreach (Renderer r in player.VRAvatar.GetComponentsInChildren<Renderer>())
        {
            r.enabled = false;
        }
        foreach (Renderer r in healthBar.GetComponentsInChildren<Renderer>())
        {
            r.enabled = false;
        }
        healthBar.gameObject.SetActive(false);

        //starts the next coroutine
        yield return new WaitForSeconds(waitTime);

        healthBar.GetComponent<Renderer>().enabled = true;

        foreach (Renderer r in player.VRAvatar.GetComponentsInChildren<Renderer>())
        {
            r.enabled = true;
        }
        foreach (Renderer r in healthBar.GetComponentsInChildren<Renderer>())
        {
            r.enabled = true;
        }
        healthBar.gameObject.SetActive(true);

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
