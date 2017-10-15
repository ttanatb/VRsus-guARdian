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
    public GameObject winAreaPrefab;

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


    private bool isPlacing = false;

    private void Awake()
    {
        if (!canvas)
            canvas = GameObject.Find("Canvas").transform;
    }

    private void Start()
    {
        player = GetComponent<Player>();
        if (player.PlayerType == PlayerType.AR)
        {
            avatar = player.ARAvatar.transform;
        }
        else
        {
            avatar = player.VRAvatar.transform;
        }

        if (!isLocalPlayer)
        {
            healthBar = Instantiate(healthBarPrefab).GetComponent<HealthBar>();
            healthBar.Init(this, player.PlayerType, avatar);
        }
        else
        {
            healthBarUI = Instantiate(healthBarUIPrefab, canvas).GetComponent<HealthBarUI>();
            healthBarUI.Init(this);

            if (player.PlayerType == PlayerType.AR)
                CanvasManager.Instance.SetUpGoalPlacingUI(this);
        }

        prevPos = avatar.position;
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
        for (int i = 0; i < hurtFlashCount; i++)
        {
            if (hurtFlashes[i])
                Destroy(hurtFlashes[i].gameObject);
        }

        if (healthBar)
            Destroy(healthBar.gameObject);

        if (healthBarUI)
            Destroy(healthBarUI.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (player.PlayerType == PlayerType.VR)
            avatar.forward = transform.forward;

        if (!isLocalPlayer)
            return;

        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.E))
        {
            TakeDamage();
        }

        if (!IsPointerOverUIObject())
        {
            if (CheckTap())
            {
                if (isPlacing)
                {
                    CheckTapOnARPlane();
                }
                else
                {
                    CmdFire(avatar.position, avatar.forward);
                }
            }
            else if ((Input.GetMouseButtonDown(0)))
            {
                CmdCreateJumpPad(transform.position + Vector3.down * 0.01f);
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
#if UNITY_IOS
        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                if (Input.GetTouch(i).phase == TouchPhase.Began)
                    return true;
            }
        }
#endif
        return false;
    }

    void CheckTapOnARPlane()
    {
        RaycastHit hit;
        int layer = LayerMask.NameToLayer("Tower");

        if (Input.touchCount > 0)
        {
            foreach (Touch t in Input.touches)
            {
                if (t.phase == TouchPhase.Began && Physics.Raycast(Camera.main.ScreenPointToRay(t.position), out hit, layer))
                {
                    GameObject obj = Instantiate(winAreaPrefab, hit.point, Quaternion.identity);
                    NetworkServer.Spawn(obj);

                    isPlacing = false;
                    return;
                }
            }

        }
        return;
    }

    public UnityAction GetActionToSwitchToPlacingMode()
    {
        UnityAction action = () =>
        {
            isPlacing = true;
        };

        return action;
    }

    private bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    [Command]
    void CmdFire(Vector3 pos, Vector3 forward)
    {
        GameObject bulletObj = null;
        if (player.PlayerType == PlayerType.AR)
        {
            bulletObj = Instantiate(bulletPrefab,
            avatar.position + avatar.localScale.z * avatar.forward,
            Quaternion.identity);
            bulletObj.GetComponent<Rigidbody>().velocity = avatar.forward * bulletSpeed;// + (avatar.position - prevPos);		
        }
        else
        {
            bulletObj = Instantiate(bulletPrefab,
            transform.position + avatar.localScale.z * transform.forward,
            Quaternion.identity);
            bulletObj.GetComponent<Rigidbody>().velocity = transform.forward * bulletSpeed;// + (avatar.position - prevPos);		

        }
        //bulletObj = Instantiate(bulletPrefab, pos, Quaternion.identity);
        //bulletObj.GetComponent<Rigidbody>().velocity = forward * bulletSpeed;
        NetworkServer.Spawn(bulletObj);
        bulletObj.GetComponent<Bullet>().Init(player.PlayerType, isLocalPlayer);
        Destroy(bulletObj, bulletTimer);
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
        if (!isServer)
            return;

        health--;


        if (health < 1)
        {
            //health = maxHealth;
            //isDead = true;
            //RpcRespawn();
        }
    }

    [ClientRpc]
    void RpcRespawn()
    {
        if (isLocalPlayer)
        {
            if (GetComponent<Player>().PlayerType == PlayerType.VR)
                transform.position = Vector3.zero;
        }
    }
}
