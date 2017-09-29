using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.iOS;

public class Combat : NetworkBehaviour
{
    public const int maxHealth = 3;

    [SyncVar]
    public int health = maxHealth;

    public GameObject bulletPrefab;
    public float bulletSpeed = 1f;
    public float bulletTimer = 2f;

    public Text playerHealth;
    public Text enemyHealth;

    public GameObject HurtScreenPrefab;

    private HurtFlash[] hurtFlashes;
    private int hurtFlashCount = 7;
    private int hurtFlashIndex = 0;

    private int prevHealth;

    private Player player;

    void Awake()
    {
        hurtFlashes = new HurtFlash[hurtFlashCount];
        Transform canvas = GameObject.Find("Canvas").transform;
        for(int i = 0; i < hurtFlashCount; i++)
        {
            hurtFlashes[i] = Instantiate(HurtScreenPrefab, canvas).GetComponent<HurtFlash>();
        }
        //playerHealth = GameObject.Find("Player Health").GetComponent<Text>();
        //enemyHealth = GameObject.Find("Enemy Health").GetComponent<Text>();
    }

    [Command]
    void CmdFire()
    {
        GameObject bullet = null;
        if (player.PlayerType == PlayerType.AR)
        {
            bullet = Instantiate(bulletPrefab, Camera.main.transform.position + Camera.main.transform.forward / 15f, Quaternion.identity);
            bullet.GetComponent<Rigidbody>().velocity = Camera.main.transform.forward * bulletSpeed;
        }
        else
        {
            bullet = Instantiate(bulletPrefab, transform.position + transform.forward / 15f, Quaternion.identity);
            bullet.GetComponent<Rigidbody>().velocity = transform.forward * bulletSpeed;
        }

        NetworkServer.Spawn(bullet);
        Destroy(bullet, bulletTimer);
    }

    private void Start()
    {
        player = GetComponent<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer)
            return;

        if (prevHealth != health)
        {
            hurtFlashes[hurtFlashIndex].FlashRed();

            hurtFlashIndex++;
            if (hurtFlashIndex > hurtFlashCount - 1)
                hurtFlashIndex = 0;

            prevHealth = health;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            TakeDamage();
        }

        if (Input.GetMouseButtonDown(0) || CheckTap())
        {
            CmdFire();
        }
    }

    bool CheckTap()
    {
        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                if (Input.GetTouch(i).phase == TouchPhase.Began)
                    return true;
            }
        }

        return false;
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
