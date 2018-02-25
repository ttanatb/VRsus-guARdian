using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class EnvironmentObject : NetworkBehaviour
{
    private float radius;

    public EnvironmentData environmentData;
    private EnvironmentObjectData environmentObjectData;

    public ParticleSystem pSystem;
    private Animator anim;

    public float Radius
    {
        get { return radius; }
        set { radius = value; }
    }

    private void Init()
    {
        int count = environmentObjectData.capColDatas.Length;
        bool collider = false;
        for (int i = 0; i < count; i++)
        {
            CapsuleColliderData data = environmentObjectData.capColDatas[i];
            CapsuleCollider col = gameObject.AddComponent<CapsuleCollider>();
            col.center = data.center;
            col.radius = data.radius;
            col.height = data.height;
            collider = true;
        }

        count = environmentObjectData.boxColDatas.Length;
        for (int i = 0; i < count; i++)
        {
            BoxColliderData data = environmentObjectData.boxColDatas[i];
            BoxCollider col = gameObject.AddComponent<BoxCollider>();
            col.center = data.center;
            col.size = data.size;
            collider = true;
        }

        if (collider)
        {
            gameObject.AddComponent<Rigidbody>().isKinematic = true;
            //GetComponent<NetworkTransform>().transformSyncMode = NetworkTransform.TransformSyncMode.SyncRigidbody3D;
        }


        anim = GetComponent<Animator>();
        anim.SetTrigger("StartEntrance");

        pSystem.Play();

        count = environmentObjectData.meshMatDatas.Length;
        for (int i = 0; i < count; i++)
        {
            MeshMatData data = environmentObjectData.meshMatDatas[i];
            GameObject child = new GameObject("Mesh");
            child.transform.SetParent(transform);
            child.transform.localScale *= data.scale;
            child.transform.localPosition = data.posOffset;
            child.transform.localRotation = Quaternion.identity;
            child.AddComponent<MeshRenderer>().sharedMaterials = data.mats;
            child.AddComponent<MeshFilter>().mesh = data.mesh;
        }

        radius = environmentObjectData.radius;

    }

    [ClientRpc]
    public void RpcInit(int type, int index)
    {
        switch(type)
        {
            case 0:
                environmentObjectData = environmentData.decorDataList[index];
                break;
            case 1:
                environmentObjectData = environmentData.structureDataList[index];
                break;
            default:
                environmentObjectData = environmentData.landMarkDataList[index];
                break;
        }

        Invoke("Init", 5f);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan * 0.3f;
        Gizmos.DrawSphere(transform.position, radius);
    }
}
