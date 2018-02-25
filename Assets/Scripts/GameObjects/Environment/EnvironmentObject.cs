using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class EnvironmentObject : MonoBehaviour
{
    private float radius;

    public float Radius
    {
        get { return radius; }
        set { radius = value; }
    }

    public void Initialize(EnvironmentObjectData decorEnvObj)
    {
        int count = decorEnvObj.capColDatas.Count;
        bool collider = false;
        for (int i = 0; i < count; i++)
        {
            CapsuleColliderData data = decorEnvObj.capColDatas[i];
            CapsuleCollider col = gameObject.AddComponent<CapsuleCollider>();
            col.center = data.center;
            col.radius = data.radius;
            col.height = data.height;
            collider = true;
        }

        count = decorEnvObj.boxColDatas.Count;
        for (int i = 0; i < count; i++)
        {
            BoxColliderData data = decorEnvObj.boxColDatas[i];
            BoxCollider col = gameObject.AddComponent<BoxCollider>();
            col.center = data.center;
            col.size = data.size;
            collider = true;
        }

        if (collider)
        {
            gameObject.AddComponent<Rigidbody>().isKinematic = true;
            GetComponent<NetworkTransform>().transformSyncMode = NetworkTransform.TransformSyncMode.SyncRigidbody3D;
        }

        count = decorEnvObj.meshMatDatas.Count;
        for (int i = 0; i < count; i++)
        {
            MeshMatData data = decorEnvObj.meshMatDatas[i];
            GameObject child = new GameObject("Mesh");
            child.transform.SetParent(transform);
            child.transform.localScale *= data.scale;
            child.transform.localPosition = data.posOffset;
            child.AddComponent<MeshRenderer>().sharedMaterials = data.mats;
            child.AddComponent<MeshFilter>().mesh = data.mesh;
        }

        radius = decorEnvObj.radius;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan * 0.3f;
        Gizmos.DrawSphere(transform.position, radius);
    }
}
