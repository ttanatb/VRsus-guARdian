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

    /// <summary>
    /// Henlo
    /// </summary>
    /// <param name="radius"></param>
    public EnvironmentObjectData(float radius = 0)
    {
        meshMatDatas = new MeshMatData[1];
        meshMatCount = 0;
        boxColDatas = new BoxColliderData[0];
        capColDatas = new CapsuleColliderData[0];
        this.radius = radius;
        isLandMark = false;
    }

    public void AddMeshMat(Mesh mesh, Material[] mats, float scale, Vector3 pos)
    {
        if (meshMatCount >= meshMatDatas.Length)
        {
            MeshMatData[] newArr = new MeshMatData[meshMatDatas.Length + 1];
            meshMatDatas = newArr;
        }

        meshMatDatas[meshMatCount] = new MeshMatData(mesh, mats, scale, pos);
        meshMatCount++;
    }

    public void SetBoxCols(BoxCollider[] collider)
    {
        boxColDatas = new BoxColliderData[collider.Length];
        for (int i = 0; i < collider.Length; i++)
        {
            boxColDatas[i] = new BoxColliderData(collider[i].center, collider[i].size);
        }
    }

    public void AddCapCols(CapsuleCollider[] collider)
    {
        capColDatas = new CapsuleColliderData[collider.Length];
        for (int i = 0; i < collider.Length; i++)
        {
            capColDatas[i] = new CapsuleColliderData(collider[i].center,
                collider[i].radius,
                collider[i].height);

        }
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

            if (min.x < minX) minX = min.x;
            if (max.x > maxX) maxX = max.x;

            if (min.z < minZ) minZ = min.z;
            if (max.z > maxZ) maxZ = max.z;
        }

        radius = Mathf.Max(maxX - minX, maxZ - minZ) / 2f;
    }
}

[System.Serializable]
public struct MeshMatData
{
    public Mesh mesh;
    public Material[] mats;
    public float scale;
    public Vector3 posOffset;

    public MeshMatData(Mesh mesh, Material[] mats, float scale, Vector3 pos)
    {
        this.mesh = mesh;
        this.mats = mats;
        this.scale = scale;
        posOffset = pos;
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