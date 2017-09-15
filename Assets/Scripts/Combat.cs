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

	public bool isDead = false;

#if UNITY_IOS
    private Transform ARTransform;
#endif

	void Awake()
	{
		playerHealth = GameObject.Find ("Player Health").GetComponent<Text> ();
		enemyHealth = GameObject.Find ("Enemy Health").GetComponent<Text> ();
	}

    [Command]
    void CmdFire()
    {
		
#if UNITY_IOS
        GameObject bullet = Instantiate(bulletPrefab, ARTransform.position + ARTransform.forward / 12f, Quaternion.identity);
		bullet.GetComponent<Bullet> ().owner = "ARPlayer";
#else
        GameObject bullet = Instantiate(bulletPrefab, transform.position + transform.forward / 15f, Quaternion.identity);
		bullet.GetComponent<Bullet> ().owner = "Player";
#endif

        bullet.GetComponent<Rigidbody>().velocity = transform.forward * bulletSpeed;

        NetworkServer.Spawn(bullet);

        Destroy(bullet, bulletTimer);
    }

    private void Start()
    {
#if UNITY_IOS
		ARTransform = GetComponent<Player>().ARCamera.transform;
#endif
    }

    // Update is called once per frame
    void Update()
    {
		if (!isLocalPlayer) {
			enemyHealth.text = "Enemy Health: " + health;
			return;
		} else {
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
			isDead = true;
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
