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
        bool collider = false;

        int count = environmentObjectData.meshMatDatas.Length;
        for (int i = 0; i < count; i++)
        {
            //Debug.Log(i);

            MeshMatData data = environmentObjectData.meshMatDatas[i];
            GameObject child = new GameObject("Mesh");
            child.transform.SetParent(transform);
            child.transform.localScale *= data.scale;
            child.transform.localPosition = data.posOffset;
            child.transform.localRotation = Quaternion.identity;
            child.AddComponent<MeshRenderer>().sharedMaterials = data.mats;
            child.AddComponent<MeshFilter>().mesh = data.mesh;

            for (int b=0; b<data.boxData.Length; b++)
            {
                BoxColliderData boxData = environmentObjectData.boxColDatas[data.boxData[b]];
                BoxCollider col = child.AddComponent<BoxCollider>();

                col.center = boxData.center;
                col.size = boxData.size;
                collider = true;
            }

            for (int c = 0; c < data.capsuleData.Length; c++)
            {
                CapsuleColliderData capData = environmentObjectData.capColDatas[data.capsuleData[c]];
                CapsuleCollider col = child.AddComponent<CapsuleCollider>();
                col.center = capData.center;
                col.radius = capData.radius;
                col.height = capData.height;
                collider = true;
            }
        }

        if (collider)
        {
            gameObject.AddComponent<Rigidbody>().isKinematic = true;
            //GetComponent<NetworkTransform>().transformSyncMode = NetworkTransform.TransformSyncMode.SyncRigidbody3D;
        }

        radius = environmentObjectData.radius;

        anim = GetComponent<Animator>();
        anim.SetTrigger("StartEntrance");
        pSystem.Play();
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

    public void Init(int type, int index)
    {
        switch (type)
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

        Init();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan * 0.3f;
        Gizmos.DrawSphere(transform.position, radius);
    }
}
