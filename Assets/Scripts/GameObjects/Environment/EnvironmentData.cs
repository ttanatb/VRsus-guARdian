using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentData : ScriptableObject
{
    public EnvironmentObjectData[] decorDataList;
    public EnvironmentObjectData[] structureDataList;
    public EnvironmentObjectData[] landMarkDataList;
}

[System.Serializable]
public struct EnvironmentObjectData
{
    public MeshMatData[] meshMatDatas;// = new List<MeshMatData>();
    int meshMatCount;// = 1;
    public BoxColliderData[] boxColDatas;
    public CapsuleColliderData[] capColDatas;
    public float radius;
    public bool isLandMark;
    public float rotY;

    public EnvironmentObjectData(float radius = 0)
    {
        meshMatDatas = new MeshMatData[1];
        meshMatCount = 0;
        rotY = 0;
        boxColDatas = new BoxColliderData[0];
        capColDatas = new CapsuleColliderData[0];
        this.radius = radius;
        isLandMark = false;
    }

    public void AddMeshMat(Mesh mesh, Material[] mats, float scale, Vector3 pos, Quaternion rot)
    {
        if (meshMatCount >= meshMatDatas.Length)
        {
            MeshMatData[] newArr = new MeshMatData[meshMatDatas.Length + 1];
            for (int i = 0; i < meshMatDatas.Length; i++)
            {
                newArr[i] = meshMatDatas[i];
            }
            meshMatDatas = newArr;
        }

        meshMatDatas[meshMatCount] = new MeshMatData(mesh, mats, scale, pos, rot);
        meshMatCount++;

        for (int i = 0; i < meshMatDatas.Length; i++)
        {
            Debug.Log("Added " + meshMatDatas[i].mesh + ", " + meshMatDatas[i].mats[0]);
        }
    }

    public void AddBoxCols(BoxCollider[] collider, float scale, Vector3 offset)
    {
        BoxColliderData[] newBoxColDatas = new BoxColliderData[collider.Length + boxColDatas.Length];
        meshMatDatas[meshMatDatas.Length - 1].boxData = new int[collider.Length];

        Debug.Log(meshMatDatas[meshMatDatas.Length - 1].boxData = new int[collider.Length]);

        for (int i = 0; i < collider.Length; i++)
        {
            newBoxColDatas[i] = new BoxColliderData(collider[i].center, collider[i].size);

            meshMatDatas[meshMatDatas.Length - 1].boxData[i] = boxColDatas.Length + i;
        }

        for (int i = collider.Length; i < newBoxColDatas.Length; i++)
        {
            newBoxColDatas[i] = boxColDatas[i - collider.Length];
        }

        boxColDatas = newBoxColDatas;
    }

    public void AddCapCols(CapsuleCollider[] collider, float scale, Vector3 offset)
    {
        CapsuleColliderData[] newCapColDatas = new CapsuleColliderData[collider.Length + capColDatas.Length];
        meshMatDatas[meshMatDatas.Length - 1].capsuleData = new int[collider.Length];

        for (int i = 0; i < collider.Length; i++)
        {
            newCapColDatas[i] = new CapsuleColliderData(collider[i].center,
                collider[i].radius,
                collider[i].height);

            meshMatDatas[meshMatDatas.Length - 1].capsuleData[i] = capColDatas.Length + i;
        }

        for (int i = collider.Length; i < newCapColDatas.Length; i++)
        {
            newCapColDatas[i] = capColDatas[i - collider.Length];
        }

        capColDatas = newCapColDatas;
    }

    public void CalcRadius()
    {
        float minX, minZ, maxX, maxZ;
        minX = minZ = float.MaxValue;
        maxX = maxZ = float.MinValue;
        for (int i = 0; i < meshMatDatas.Length; i++)
        {
            Bounds b = meshMatDatas[i].mesh.bounds;
            Vector3 min = b.min * meshMatDatas[i].scale + meshMatDatas[i].posOffset;
            Vector3 max = b.max * meshMatDatas[i].scale + meshMatDatas[i].posOffset;

            minX = Mathf.Min(minX, min.x);
            maxX = Mathf.Max(maxX, max.x);

            minZ = Mathf.Min(minZ, min.z);
            maxZ = Mathf.Max(maxZ, max.z);
        }

        radius = Mathf.Max(Mathf.Abs(maxX), Mathf.Abs(minX), Mathf.Abs(minZ), Mathf.Abs(maxZ));

        ReCenter((minX + maxX) / 2.0f, (minZ + maxZ) / 2.0f);
    }

    public void ReCenter(float centerX, float centerZ)
    {
        for (int i = 0; i < meshMatDatas.Length; i++)
        {
            meshMatDatas[i].posOffset -= new Vector3(centerX, 0, centerZ);
        }
    }
}

[System.Serializable]
public struct MeshMatData
{
    public Mesh mesh;
    public Material[] mats;
    public float scale;
    public Vector3 posOffset;
    public Quaternion rotation;

    public int[] boxData;
    public int[] capsuleData;

    public MeshMatData(Mesh mesh, Material[] mats, float scale, Vector3 pos, Quaternion rotation)
    {
        this.mesh = mesh;
        this.mats = mats;
        this.scale = scale;
        posOffset = pos;
        this.rotation = rotation;

        boxData = null;
        capsuleData = null;
    }
}

[System.Serializable]
public struct CapsuleColliderData
{
    public Vector3 center;
    public float radius;
    public float height;
    public CapsuleColliderData(Vector3 center, float radius, float height)
    {
        this.center = center;
        this.radius = radius;
        this.height = height;
    }
}

[System.Serializable]
public struct BoxColliderData
{
    public Vector3 center;
    public Vector3 size;
    public BoxColliderData(Vector3 center, Vector3 size)
    {
        this.center = center;
        this.size = size;
    }
}