using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Deals with health, invulnerability, and relic count
/// 
/// Author: Tanat Boozayaangool
/// </summary>
public class VRCombat : PlayerComponent
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
    public const float MAX_INVUL_TIME = 1.75f;

    [SyncVar]
    private bool isInvulnerable = false;

    //visual feedback for getting hurt
    public GameObject HurtScreenPrefab;
    private HurtFlash[] hurtFlashes;
    private int hurtFlashCount = 1;
    private int hurtFlashIndex = 0;

    private Renderer[] playerRenderers;

    private float timer = 0f;
    private float fadeSpeed = 0.1f;
    private Color transparent = new Color(1, 1, 1, 0);
    private Color solid = new Color(1, 1, 1, 0);
    private Fader fader;

    //relic stuff
    [SyncVar]
    private int relicCount = 0;

    public bool IsInvulnerable
    {
        get { return isInvulnerable; }
        set { isInvulnerable = value; }
    }
    #endregion

    #region Init & Destruction
    private void Start()
    {
        canvas = CanvasManager.Instance.transform;
        avatar = player.avatar.transform;

        playerRenderers = player.renderersToDisbale;
        if (!isLocalPlayer)
        {
            fader = GetComponentInChildren<Fader>();
            //healthBar = Instantiate(healthBarPrefab).GetComponent<HealthBar>();
            //healthBar.Init(this, playerType, avatar);
        }
        else
        {
            hurtFlashes = new HurtFlash[hurtFlashCount];
            for (int i = 0; i < hurtFlashCount; i++)
                hurtFlashes[i] = Instantiate(HurtScreenPrefab, canvas).GetComponent<HurtFlash>();
            CanvasManager.Instance.InitHealthEnergyBar(this);
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
            RpcFlashRed();
            isInvulnerable = true;
            fader.Fade(MAX_INVUL_TIME);

            CanvasManager.Instance.SetMessage("The intruder was hit! Life total at " + (int)(health / (float)maxHealth * 100f) + "%");
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
        while (timer < 1f)
        {
            timer += fadeSpeed;
            for (int i = 0; i < playerRenderers.Length; i++)
            {
                playerRenderers[i].material.SetColor("_Color", Color.Lerp(solid, transparent, timer));
            }
            yield return new WaitForEndOfFrame();
        }

        //waits before fading in
        yield return new WaitForSeconds(waitTime);

        while (timer < 2f)
        {
            timer += fadeSpeed;
            for (int i = 0; i < playerRenderers.Length; i++)
            {
                playerRenderers[i].material.SetColor("_Color", Color.Lerp(transparent, solid, timer - 1f));
            }
            yield return new WaitForEndOfFrame();
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
