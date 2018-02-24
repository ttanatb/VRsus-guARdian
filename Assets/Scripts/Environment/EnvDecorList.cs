using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvDecorList : ScriptableObject
{
    public List<DecorEnvObj> decorPrefabList;
}

[System.Serializable]
public struct DecorEnvObj
{
    public Mesh mesh;
    public Material mat;
    public float radius;

    public DecorEnvObj(Mesh mesh, Material mat, float radius)
    {
        this.mesh = mesh;
        this.mat = mat;
        this.radius = radius;
    }
}