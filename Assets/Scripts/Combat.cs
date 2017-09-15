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

	private Rigidbody rb = null;

    void Awake()
    {
        playerHealth = GameObject.Find("Player Health").GetComponent<Text>();
        enemyHealth = GameObject.Find("Enemy Health").GetComponent<Text>();
    }

    [Command]
    void CmdFire()
    {
        GameObject bullet = null;
		if (GetComponent<Rigidbody>())
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
		rb = GetComponent<Rigidbody> ();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer)
        {
            enemyHealth.text = "Enemy Health: " + health;
            return;
        }
        else
        {
            playerHealth.text = "Health: " + health;
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
