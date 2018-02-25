using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentData : ScriptableObject
{
    public List<EnvironmentObjectData> decorDataList;
    public List<EnvironmentObjectData> structuresDataList;
    public List<EnvironmentObjectData> landMarkDataList;
}

[System.Serializable]
public struct EnvironmentObjectData
{
    public List<MeshMatData> meshMatDatas;// = new List<MeshMatData>();
    public List<BoxColliderData> boxColDatas;
    public List<CapsuleColliderData> capColDatas;
    public float radius;
    public bool isLandMark;

    /// <summary>
    /// Henlo
    /// </summary>
    /// <param name="radius"></param>
    public EnvironmentObjectData(float radius = 0)
    {
        meshMatDatas = new List<MeshMatData>();
        boxColDatas = new List<BoxColliderData>();
        capColDatas = new List<CapsuleColliderData>();
        this.radius = radius;
        isLandMark = false;
    }

    public void AddMeshMat(Mesh mesh, Material[] mats, float scale, Vector3 pos)
    {
        meshMatDatas.Add(new MeshMatData(mesh, mats, scale, pos));
    }

    public void AddBoxCols(BoxCollider[] collider)
    {
        for (int i = 0; i < collider.Length; i++)
        {
            boxColDatas.Add(new BoxColliderData(collider[i].center, collider[i].size));
        }
    }

    public void AddCapCols(CapsuleCollider[] collider)
    {
        for (int i = 0; i < collider.Length; i++)
        {
            capColDatas.Add(new CapsuleColliderData(collider[i].center,
                collider[i].radius,
                collider[i].height));
        }
    }

    public void CalcRadius()
    {
        float minX, minZ, maxX, maxZ;
        minX = minZ = float.MaxValue;
        maxX = maxZ = float.MinValue;
        for (int i = 0; i < meshMatDatas.Count; i++)
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