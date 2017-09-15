using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

#if UNITY_IOS
    private Transform ARTransform;
#endif

    [Command]
    void CmdFire()
    {
#if UNITY_IOS
        GameObject bullet = Instantiate(bulletPrefab, ARTransform.position + ARTransform.forward / 12f, Quaternion.identity);
#else
        GameObject bullet = Instantiate(bulletPrefab, transform.position + transform.forward / 15f, Quaternion.identity);
#endif
        bullet.GetComponent<Rigidbody>().velocity = transform.forward * bulletSpeed;

        NetworkServer.Spawn(bullet);

        Destroy(bullet, bulletTimer);
    }

    private void Start()
    {
#if UNITY_IOS
    ARTransform = GetComponentInChildren<ARAvatar>().transform;
#endif
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer)
            return;

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
            health = maxHealth;
            RpcRespawn();
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
